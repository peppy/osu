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
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCarouselV2 : CompositeDrawable
    {
        /// <summary>
        /// Whether an asynchronous sort / group operation is currently underway.
        /// </summary>
        public bool IsSorting => !sortTask.IsCompleted;

        /// <summary>
        /// The number of displayable beatmap difficulties currently being tracked (before sorting).
        /// </summary>
        public int BeatmapsTracked => carouselItems.Count;

        /// <summary>
        /// The number of carousel items currently in rotation for display.
        /// </summary>
        public int DisplayableItems => displayCarouselItems?.Count ?? 0;

        /// <summary>
        /// The number of items currently actualised into drawables.
        /// </summary>
        public int VisibleItems => panels.CountDisplayedPanels;

        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly List<CarouselItem> carouselItems = new List<CarouselItem>();

        private List<CarouselItem>? displayCarouselItems;

        private DoubleScrollContainer panels = null!;

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
                panels = new DoubleScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
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
                    carouselItems.AddRange(newBeatmapSets!.SelectMany(s => s.Beatmaps).Select(b => new CarouselItem(b)));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    foreach (var set in beatmapSetInfos!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            carouselItems.RemoveAll(i => i.Model is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Reset:
                    carouselItems.Clear();
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

        private void runSortBackground() => Scheduler.AddOnce(() =>
        {
            // TODO: some kind of timed debounce for cases where a lot of beatmap store operations are running.
            sortTask = performSortGroupDisplay();
        });

        private async Task performSortGroupDisplay()
        {
            // TODO: remove eventually
            Debug.Assert(SynchronizationContext.Current != null);

            var criteria = Criteria;
            var cts = new CancellationTokenSource();

            lock (this)
            {
                cancellationSource.Cancel();
                cancellationSource = cts;
            }

            var items = new List<CarouselItem>(carouselItems);

            await Task.Run(async () =>
            {
                await sort(items, criteria, cts.Token).ConfigureAwait(false);
                await group(items, criteria, cts.Token).ConfigureAwait(false);
                await updateYPositions(items, cts.Token).ConfigureAwait(false);
            }, cts.Token).ConfigureAwait(true);

            if (cts.Token.IsCancellationRequested)
                return;

            displayCarouselItems = items;
        }

        private static async Task sort(List<CarouselItem> items, FilterCriteria criteria, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            Thread.Sleep(1000);
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
            {
                panels.Clear(false);
                return;
            }

            updateDisplayedRange();
        }

        #region Display range handling

        /// <summary>
        /// Height of the area above the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedTop { get; set; }

        /// <summary>
        /// Height of the area below the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedBottom { get; set; }

        private (int first, int last) displayedRange;

        private readonly CarouselItem carouselBoundsItem = new CarouselItem(null!);

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => (float)(panels.Current + DrawHeight + BleedBottom);

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => (float)(panels.Current - BleedTop);

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
                panels.Clear(false);
                displayedRange = range;

                for (int i = range.firstIndex; i <= range.lastIndex; i++)
                {
                    var item = displayCarouselItems[i];
                    var carouselPanel = carouselPanelPool.Get(panel => panel.Item = item);

                    item.Drawable = carouselPanel;
                    panels.Add(carouselPanel);
                }
            }
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

            // as we can't be 100% sure on the size of individual carousel drawables,
            // always play it safe and extend bounds by one.
            firstIndex = Math.Max(0, firstIndex - 1);
            lastIndex = Math.Clamp(lastIndex + 1, firstIndex, Math.Max(0, displayCarouselItems.Count - 1));

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

            public readonly List<CarouselItem> Children = new List<CarouselItem>();

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
                        return $"Difficulty: {bi.DifficultyName} ({bi.StarRating}*)";

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
                set
                {
                    item = value;

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

        internal partial class DoubleScrollContainer : BasicScrollContainer
        {
            public int CountDisplayedPanels => layoutContent.Count;

            private readonly Container<CarouselPanel> layoutContent;

            public DoubleScrollContainer()
            {
                // Managing our own custom layout within ScrollContent causes feedback with internal ScrollContainer calculations,
                // so we must maintain one level of separation from ScrollContent.
                base.Add(layoutContent = new Container<CarouselPanel>
                {
                    Name = "Layout content",
                    RelativeSizeAxes = Axes.X,
                });
            }

            public override void Clear(bool disposeChildren)
            {
                layoutContent.Height = 0;
                layoutContent.Clear(disposeChildren);
            }

            public override void Add(Drawable drawable)
            {
                if (drawable is not CarouselPanel panel)
                    throw new InvalidOperationException();

                Add(panel);
            }

            public void Add(CarouselPanel drawable)
            {
                if (drawable is not CarouselPanel panel)
                    throw new InvalidOperationException();

                layoutContent.Height = (float)Math.Max(layoutContent.Height, panel.YPosition + panel.DrawHeight);
                layoutContent.Add(drawable);
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

                foreach (var d in layoutContent)
                    d.Y = (float)(d.YPosition + scrollableExtent);
            }
        }
    }
}
