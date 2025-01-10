// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class Carousel<T> : CompositeDrawable
        where T : PoolableDrawable, ICarouselPanel, new()
    {
        /// <summary>
        /// Height of the area above the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedTop { get; set; }

        /// <summary>
        /// Height of the area below the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedBottom { get; set; }

        /// <summary>
        /// Whether an asynchronous sort / group operation is currently underway.
        /// </summary>
        public bool IsSorting => !sortTask.IsCompleted;

        /// <summary>
        /// The number of displayable items currently being tracked (before sorting).
        /// </summary>
        public int ItemsTracked => Items.Count;

        /// <summary>
        /// The number of carousel items currently in rotation for display.
        /// </summary>
        public int DisplayableItems => displayCarouselItems?.Count ?? 0;

        /// <summary>
        /// The number of items currently actualised into drawables.
        /// </summary>
        public int VisibleItems => scroll.Panels.Count;

        public IEnumerable<ICarouselFilter> Filters { get; init; } = Enumerable.Empty<ICarouselFilter>();

        protected readonly BindableList<CarouselItem> Items = new BindableList<CarouselItem>();

        private List<CarouselItem>? displayCarouselItems;

        private DoubleScrollContainer scroll = null!;

        private readonly DrawablePool<T> carouselPanelPool = new DrawablePool<T>(100);

        /// <summary>
        /// When a new request arrives to change filtering, we wait a small period before beginning processing.
        /// Regardless of any external debouncing, this is a safety measure to avoid triggering too many threaded operations.
        ///
        /// Based on how fast we want this to respond, this could be reduced down to 50 ms or so.
        /// </summary>
        private const int debounce_delay = 100;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                carouselPanelPool,
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                scroll = new DoubleScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = false,
                }
            };
        }

        private Task sortTask = Task.CompletedTask;
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();

        protected void PerformSort() => Scheduler.AddOnce(() => sortTask = performSortGroupDisplay());

        private async Task performSortGroupDisplay()
        {
            Debug.Assert(SynchronizationContext.Current != null);

            var cts = new CancellationTokenSource();

            lock (this)
            {
                cancellationSource.Cancel();
                cancellationSource = cts;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<CarouselItem> items = new List<CarouselItem>(Items);

            await Task.Run(async () =>
            {
                try
                {
                    // brief debounce
                    log("New changes to handle");
                    await Task.Delay(debounce_delay, cts.Token).ConfigureAwait(false);

                    foreach (var filter in Filters)
                    {
                        log($"Performing {filter.Name}");
                        items = await filter.Run(items, cts.Token).ConfigureAwait(false);
                    }

                    log("Updating Y positions");
                    await updateYPositions(items, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    log("Cancelled due to newer request arriving");
                }
            }, cts.Token).ConfigureAwait(true);

            if (cts.Token.IsCancellationRequested)
                return;

            log("Items ready for display");
            displayCarouselItems = items.ToList();
            displayedRange = null;

            void log(string text) => Logger.Log($"Carousel[op {cts.GetHashCode().ToString().Substring(0, 5)}] {stopwatch.ElapsedMilliseconds} ms: {text}");
        }

        private async Task updateYPositions(IEnumerable<CarouselItem> carouselItems, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            const float spacing = 10;
            float yPos = 0;

            foreach (var item in carouselItems)
            {
                item.CarouselYPosition = yPos;
                yPos += item.DrawHeight + spacing;
            }
        }, cancellationToken).ConfigureAwait(false);

        protected override void Update()
        {
            base.Update();

            if (displayCarouselItems == null)
                return;

            updateDisplayedRange();
        }

        #region Display range handling

        private (int first, int last)? displayedRange;

        private readonly CarouselItem carouselBoundsItem = new BoundsCarouselItem();

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => (float)(scroll.Current + DrawHeight + BleedBottom);

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => (float)(scroll.Current - BleedTop);

        /// <summary>
        /// Extend the range to update positions / retrieve pooled drawables outside the visible range.
        /// </summary>
        private const float distance_offscreen_to_preload = 0; // 768

        private void updateDisplayedRange()
        {
            Debug.Assert(displayCarouselItems != null);

            var range = getDisplayRange();

            if (range != displayedRange)
            {
                Logger.Log($"Updating displayed range of carousel from {displayedRange} to {range}");

                displayedRange = range;

                List<CarouselItem> toDisplay = range.last - range.first == 0
                    ? new List<CarouselItem>()
                    : displayCarouselItems.GetRange(range.first, range.last - range.first + 1);

                // Iterate over all panels which are already displayed and figure which need to be displayed / removed.
                foreach (var panel in scroll.Panels)
                {
                    // The case where we're intending to display this panel, but it's already displayed.
                    // Note that we **must compare the model here** as the CarouselItems may be fresh instances due to a sort / group operation.
                    var existing = toDisplay.FirstOrDefault(i => i.Model == panel.Item.Model);

                    if (existing != null)
                    {
                        panel.Item = existing;
                        toDisplay.Remove(existing);
                        continue;
                    }

                    // If the new display range doesn't contain the panel, it's no longer required for display.
                    expirePanelImmediately(panel);
                }

                foreach (var item in toDisplay)
                {
                    var carouselPanel = carouselPanelPool.Get(panel =>
                    {
                        panel.Item = item;
                        item.Drawable = panel;
                    });

                    scroll.Add(carouselPanel);

                    carouselPanel.FlashColour(Color4.Red, 2000);
                }

                if (displayCarouselItems.Count > 0)
                {
                    var lastItem = displayCarouselItems[^1];
                    scroll.SetLayoutHeight((float)(lastItem.CarouselYPosition + lastItem.DrawHeight));
                }
                else
                    scroll.SetLayoutHeight(0);
            }
        }

        private static void expirePanelImmediately(Drawable panel)
        {
            // TODO: double-check (copied from old implementation).
            // may want a fade effect here (could be seen if a huge change happens, like a set with 20 difficulties becomes selected).
            panel.FinishTransforms();
            panel.Expire();
        }

        private (int first, int last) getDisplayRange()
        {
            Debug.Assert(displayCarouselItems != null);

            // Find index range of all items that should be on-screen
            carouselBoundsItem.CarouselYPosition = visibleUpperBound - distance_offscreen_to_preload;
            int firstIndex = displayCarouselItems.BinarySearch(carouselBoundsItem);
            if (firstIndex < 0) firstIndex = ~firstIndex;

            carouselBoundsItem.CarouselYPosition = visibleBottomBound + distance_offscreen_to_preload;
            int lastIndex = displayCarouselItems.BinarySearch(carouselBoundsItem);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            firstIndex = Math.Max(0, firstIndex - 1);
            lastIndex = Math.Max(0, lastIndex - 1);

            return (firstIndex, lastIndex);
        }

        #endregion

        public partial class DoubleScrollContainer : OsuScrollContainer
        {
            public readonly Container<T> Panels;

            public void SetLayoutHeight(float height) => Panels.Height = height;

            public DoubleScrollContainer()
            {
                // Managing our own custom layout within ScrollContent causes feedback with public ScrollContainer calculations,
                // so we must maintain one level of separation from ScrollContent.
                base.Add(Panels = new Container<T>
                {
                    Name = "Layout content",
                    RelativeSizeAxes = Axes.X,
                });
            }

            public override void Clear(bool disposeChildren)
            {
                Panels.Height = 0;
                Panels.Clear(disposeChildren);
            }

            public override void Add(Drawable drawable)
            {
                if (drawable is not T panel)
                    throw new InvalidOperationException();

                Add(panel);
            }

            public void Add(T drawable)
            {
                if (drawable is not T)
                    throw new InvalidOperationException();

                Panels.Add(drawable);
            }

            public override double GetChildPosInContent(Drawable d, Vector2 offset)
            {
                if (d is not T panel)
                    return base.GetChildPosInContent(d, offset);

                return panel.YPosition + offset.X;
            }

            protected override void ApplyCurrentToContent()
            {
                Debug.Assert(ScrollDirection == Direction.Vertical);

                double scrollableExtent = -Current + ScrollableExtent * ScrollContent.RelativeAnchorPosition.Y;

                foreach (var d in Panels)
                    d.Y = (float)(d.YPosition + scrollableExtent);
            }
        }

        private class BoundsCarouselItem : CarouselItem
        {
            public override float DrawHeight => 0;

            public BoundsCarouselItem()
                : base(new object())
            {
            }
        }
    }

    public abstract class CarouselItem : IComparable<CarouselItem>
    {
        public readonly object Model;

        public readonly Guid ID;

        /// <summary>
        /// The current drawable representation of this item, if it's currently being displayed.
        /// </summary>
        public Drawable? Drawable { get; set; }

        public double CarouselYPosition { get; set; }

        public abstract float DrawHeight { get; }

        protected CarouselItem(object model)
        {
            Model = model;

            ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();
        }

        public int CompareTo(CarouselItem? other)
        {
            if (other == null) return 1;

            return CarouselYPosition.CompareTo(other.CarouselYPosition);
        }
    }

    public interface ICarouselPanel
    {
        double YPosition => Item?.CarouselYPosition ?? double.MinValue;

        CarouselItem? Item { get; set; }
    }

    public interface ICarouselFilter
    {
        string Name { get; }

        Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken);
    }
}
