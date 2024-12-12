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
using osu.Framework.Graphics.Shapes;
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
        public int VisibleItems => panels.Count;

        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly List<CarouselItem> carouselItems = new List<CarouselItem>();

        private List<CarouselItem>? displayCarouselItems;

        private FillFlowContainer panels = null!;

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer(Direction.Vertical)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panels = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Spacing = new Vector2(10),
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    },
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

            await sort(items, criteria, cts.Token).ConfigureAwait(true);
            await group(items, criteria, cts.Token).ConfigureAwait(true);

            if (cts.Token.IsCancellationRequested)
                return;

            display(items);
        }

        private async Task sort(List<CarouselItem> items, FilterCriteria criteria, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
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
        }

        private async Task group(List<CarouselItem> items, FilterCriteria criteria, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                // TODO: perform grouping based on FilterCriteria
            }, cancellationToken).ConfigureAwait(false);
        }

        private void display(List<CarouselItem> items)
        {
            displayCarouselItems = items;

            panels.Clear();

            foreach (var item in displayCarouselItems)
            {
                var carouselPanel = new CarouselPanel(item);

                item.Drawable = carouselPanel;
                panels.Add(carouselPanel);
            }
        }

        internal class CarouselItem
        {
            public readonly object Model;

            public readonly Guid ID;

            /// <summary>
            /// The current drawable representation of this item, if it's currently being displayed.
            /// </summary>
            public Drawable? Drawable { get; set; }

            public readonly List<CarouselItem> Children = new List<CarouselItem>();

            public CarouselItem(object model)
            {
                Model = model;

                ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();
            }

            public override string? ToString() => Model.ToString();
        }

        internal partial class CarouselPanel : CompositeDrawable
        {
            public CarouselPanel(CarouselItem item)
            {
                Size = new Vector2(500, 80);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Yellow.Darken(5),
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
}
