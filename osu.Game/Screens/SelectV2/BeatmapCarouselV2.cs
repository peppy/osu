// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCarouselV2 : CompositeDrawable
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
        /// The number of displayable beatmap difficulties currently being tracked (before sorting).
        /// </summary>
        public int BeatmapsTracked => rawCarouselItems.Count;

        /// <summary>
        /// The number of carousel items currently in rotation for display.
        /// </summary>
        public int DisplayableItems => displayCarouselItems?.Count ?? 0;

        /// <summary>
        /// The number of items currently actualised into drawables.
        /// </summary>
        public int VisibleItems => scroll.Panels.Count;

        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly List<CarouselItem> rawCarouselItems = new List<CarouselItem>();

        private List<CarouselItem>? displayCarouselItems;

        private DoubleScrollContainer scroll = null!;

        private readonly DrawablePool<CarouselPanel> carouselPanelPool = new DrawablePool<CarouselPanel>(100);

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
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

            detachedBeatmaps = beatmapStore.GetBeatmapSets(cancellationToken);
            detachedBeatmaps.BindCollectionChanged(beatmapSetsChanged, true);
        }

        private void beatmapSetsChanged(object? beatmaps, NotifyCollectionChangedEventArgs changed)
        {
            // TODO: moving management of BeatmapInfo tracking to BeatmapStore might be something we want to consider.
            // right now we are managing this locally which is a bit of added overhead.
            IEnumerable<BeatmapSetInfo>? newBeatmapSets = changed.NewItems?.Cast<BeatmapSetInfo>();
            IEnumerable<BeatmapSetInfo>? beatmapSetInfos = changed.OldItems?.Cast<BeatmapSetInfo>();

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    rawCarouselItems.AddRange(newBeatmapSets!.SelectMany(s => s.Beatmaps).Select(b => new CarouselItem(b)));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    foreach (var set in beatmapSetInfos!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            rawCarouselItems.RemoveAll(i => i.Model is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Reset:
                    rawCarouselItems.Clear();
                    break;
            }

            runSortBackground();
        }

        public FilterCriteria Criteria { get; private set; } = new FilterCriteria();

        public void Filter(FilterCriteria criteria)
        {
            Criteria = criteria;
            runSortBackground();
        }

        private Task sortTask = Task.CompletedTask;
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();

        private void runSortBackground() => Scheduler.AddOnce(() => sortTask = performSortGroupDisplay());

        private async Task performSortGroupDisplay()
        {
            Debug.Assert(SynchronizationContext.Current != null);

            var criteria = Criteria;
            var cts = new CancellationTokenSource();

            lock (this)
            {
                cancellationSource.Cancel();
                cancellationSource = cts;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            var items = new List<CarouselItem>(rawCarouselItems);

            await Task.Run(async () =>
            {
                try
                {
                    // debounce
                    log("New changes to handle");
                    await Task.Delay(500, cts.Token).ConfigureAwait(false);

                    log("Performing sort");
                    await sort(items, criteria, cts.Token).ConfigureAwait(false);

                    log("Performing group");
                    await group(items, criteria, cts.Token).ConfigureAwait(false);

                    log("Updating Y positions");
                    await updateYPositions(items, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    log("Cancelled");
                }
            }, cts.Token).ConfigureAwait(true);

            if (cts.Token.IsCancellationRequested)
                return;

            log("Items ready for display");
            displayCarouselItems = items;
            displayedRange = default;

            // As long as PanelItems are allowed to be generated arbitrarily in the async process,
            // we need to ensure that they are reattached to active drawables to avoid reconstruction.
            //
            // Doing this here is the best way I can come up with, besides from creating a dictionary or
            // other tracking method for these relationships.
            //
            // Is this an issue? Probably. It's adding a relatively expensive operation here that I'd like
            // to be able to avoid.
            foreach (var item in displayCarouselItems)
            {
                var existingPanel = scroll.Panels.FirstOrDefault(p => p.Item.Model == item.Model);
                if (existingPanel != null)
                    existingPanel.Item = item;
            }

            void log(string text) => Logger.Log($"Carousel[op {cts.GetHashCode().ToString().Substring(0, 5)}] {stopwatch.ElapsedMilliseconds} ms: {text}");
        }

        private static async Task sort(List<CarouselItem> items, FilterCriteria criteria, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            items.Sort((a, b) =>
            {
                if (a.Model is BeatmapInfo ab && b.Model is BeatmapInfo bb)
                    return -1 * ab.OnlineID.CompareTo(bb.OnlineID);

                return a.ID.CompareTo(b.ID);
            });
            // TODO: perform sort based on FilterCriteria
        }, cancellationToken).ConfigureAwait(false);

        private static async Task group(List<CarouselItem> items, FilterCriteria criteria, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            // TODO: perform grouping based on FilterCriteria

            CarouselItem? lastItem = null;

            for (int i = 0; i < items.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = items[i];

                if (item.Model is BeatmapInfo b1)
                {
                    // Add set header
                    if (lastItem == null || (lastItem.Model is BeatmapInfo b2 && b2.BeatmapSet!.OnlineID != b1.BeatmapSet!.OnlineID))
                        insertItem(b1.BeatmapSet!);
                }

                lastItem = item;

                void insertItem(object model) => items.Insert(i++, new CarouselItem(model));
            }
        }, cancellationToken).ConfigureAwait(false);

        private async Task updateYPositions(List<CarouselItem> carouselItems, CancellationToken cancellationToken) => await Task.Run(() =>
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

        private (int first, int last) displayedRange;

        private readonly CarouselItem carouselBoundsItem = new CarouselItem(null!);

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

        /// <summary>
        /// Extend the range to retain already loaded pooled drawables.
        /// </summary>
        private const float distance_offscreen_before_unload = 0; // 2048;

        private void updateDisplayedRange()
        {
            Debug.Assert(displayCarouselItems != null);

            var range = getDisplayRange();

            if (range != displayedRange)
            {
                Logger.Log($"Updating displayed range of carousel from {displayedRange} to {range}");

                displayedRange = range;

                List<CarouselItem> toDisplay = displayCarouselItems.GetRange(displayedRange.first, displayedRange.last - displayedRange.first + 1);

                // Iterate over all panels which are already displayed
                foreach (var panel in scroll.Panels)
                {
                    // The case where we're intending to display this panel, but it's already displayed.
                    if (toDisplay.Remove(panel.Item))
                        continue;

                    // TODO: is definitely not efficient.
                    // CarouselItem? newItemSameModel = displayCarouselItems.FirstOrDefault(i => i.Model == panel.Item.Model);
                    //
                    // if (newItemSameModel == null)
                    //     expirePanelImmediately(panel);
                    // else if (newItemSameModel == panel.Item)
                    // {
                    //     // nothing to do in this case
                    //     Console.WriteLine();
                    // }
                    // else if (newItemSameModel?.Model == panel.Item.Model)
                    // {
                    //     panel.Item = newItemSameModel;
                    //     toDisplay.Remove(newItemSameModel);
                    // }

                    // panel loaded as drawable but not required by visible range.
                    // remove but only if too far off-screen
                    if (panel.YPosition + panel.DrawHeight < visibleUpperBound - distance_offscreen_before_unload ||
                        panel.YPosition > visibleBottomBound + distance_offscreen_before_unload)
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

                var lastItem = displayCarouselItems[^1];
                scroll.SetLayoutHeight((float)(lastItem.CarouselYPosition + lastItem.DrawHeight));
            }
        }

        private static void expirePanelImmediately(CarouselPanel panel)
        {
            // TODO: double-check (copied from old implementation).
            // may want a fade effect here (could be seen if a huge change happens, like a set with 20 difficulties becomes selected).
            panel.FinishTransforms();
            panel.Expire();
        }

        private (int firstIndex, int lastIndex) getDisplayRange()
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

        internal class CarouselItem : IComparable<CarouselItem>
        {
            public readonly object Model;

            public readonly Guid ID;

            /// <summary>
            /// The current drawable representation of this item, if it's currently being displayed.
            /// </summary>
            public Drawable? Drawable { get; set; }

            public double CarouselYPosition { get; set; }

            public float DrawHeight => Model is BeatmapInfo ? 40 : 80;

            public CarouselItem(object model)
            {
                Model = model;

                ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();
            }

            public override string? ToString()
            {
                switch (Model)
                {
                    case BeatmapInfo bi:
                        return $"Difficulty: {bi.DifficultyName} ({bi.StarRating:N1}*)";

                    case BeatmapSetInfo si:
                        return $"{si.Metadata}";
                }

                return Model.ToString();
            }

            public int CompareTo(CarouselItem? other)
            {
                if (other == null) return 1;

                return CarouselYPosition.CompareTo(other.CarouselYPosition);
            }
        }

        internal partial class CarouselPanel : PoolableDrawable
        {
            private CarouselItem? item;

            public double YPosition => item?.CarouselYPosition ?? double.MinValue;

            public CarouselItem Item
            {
                get => item!;
                set
                {
                    bool sameModel = item?.Model == value.Model;

                    item = value;

                    if (sameModel)
                        return;

                    Size = new Vector2(500, item.DrawHeight);

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            Colour = (item.Model is BeatmapInfo ? Color4.Aqua : Color4.Yellow).Darken(5),
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuSpriteText
                        {
                            Text = item.ToString() ?? string.Empty,
                            Padding = new MarginPadding(5),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        }
                    };
                }
            }
        }

        internal partial class DoubleScrollContainer : OsuScrollContainer
        {
            public readonly Container<CarouselPanel> Panels;

            public void SetLayoutHeight(float height) => Panels.Height = height;

            public DoubleScrollContainer()
            {
                // Managing our own custom layout within ScrollContent causes feedback with internal ScrollContainer calculations,
                // so we must maintain one level of separation from ScrollContent.
                base.Add(Panels = new Container<CarouselPanel>
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
                if (drawable is not CarouselPanel panel)
                    throw new InvalidOperationException();

                Add(panel);
            }

            public void Add(CarouselPanel drawable)
            {
                if (drawable is not CarouselPanel)
                    throw new InvalidOperationException();

                Panels.Add(drawable);
            }

            public override double GetChildPosInContent(Drawable d, Vector2 offset)
            {
                if (d is not CarouselPanel panel)
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
    }
}
