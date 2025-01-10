// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
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
    public partial class BeatmapCarouselV2 : Carousel
    {
        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        public BeatmapCarouselV2()
        {
            Filters = new List<ICarouselFilter>
            {
                new Sorter(),
                new Grouper(),
            };
        }

        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
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
                    Items.AddRange(newBeatmapSets!.SelectMany(s => s.Beatmaps).Select(b => new CarouselItem(b)));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    foreach (var set in beatmapSetInfos!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            Items.RemoveAll(i => i.Model is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }

            PerformSort();
        }

        public FilterCriteria Criteria { get; private set; } = new FilterCriteria();

        public void Filter(FilterCriteria criteria)
        {
            Criteria = criteria;
            PerformSort();
        }

        private class BeatmapCarouselPanel : PoolableDrawable, ICarouselPanel
        {
            public CarouselItem? Item { get; set; }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                Debug.Assert(Item != null);

                Size = new Vector2(500, Item.DrawHeight);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = (Item.Model is BeatmapInfo ? Color4.Aqua : Color4.Yellow).Darken(5),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Text = Item.ToString() ?? string.Empty,
                        Padding = new MarginPadding(5),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    }
                };
            }
        }

        private class BeatmapCarouselItem : CarouselItem
        {
            public override float DrawHeight => Model is BeatmapInfo ? 40 : 80;

            public BeatmapCarouselItem(object model)
                : base(model)
            {
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
        }
    }

    public class Grouper : ICarouselFilter
    {
        public string Name { get; } = "Grouper";

        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            // TODO: perform grouping based on FilterCriteria

            CarouselItem? lastItem = null;

            var newItems = new List<CarouselItem>();

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.Model is BeatmapInfo b1)
                {
                    // Add set header
                    if (lastItem == null || (lastItem.Model is BeatmapInfo b2 && b2.BeatmapSet!.OnlineID != b1.BeatmapSet!.OnlineID))
                        newItems.Add(new CarouselItem(b1.BeatmapSet!));
                }

                newItems.Add(item);
                lastItem = item;
            }

            return newItems;
        }, cancellationToken).ConfigureAwait(false);
    }

    public class Sorter : ICarouselFilter
    {
        public string Name { get; } = "Sorter";

        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            return items.OrderDescending(Comparer<CarouselItem>.Create((a, b) =>
            {
                if (a.Model is BeatmapInfo ab && b.Model is BeatmapInfo bb)
                    return -1 * ab.OnlineID.CompareTo(bb.OnlineID);

                return a.ID.CompareTo(b.ID);
            }));
        }, cancellationToken).ConfigureAwait(false);
    }
}
