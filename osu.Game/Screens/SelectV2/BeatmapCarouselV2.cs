// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapCarouselV2 : CompositeDrawable
    {
        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly List<CarouselItem> carouselItems = new List<CarouselItem>();

        private FillFlowContainer panels = null!;

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            detachedBeatmaps = beatmapStore.GetBeatmapSets(cancellationToken);
            detachedBeatmaps.BindCollectionChanged(beatmapSetsChanged, true);

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

            sort();
            group();
            display();
        }

        private void sort()
        {
        }

        private void group()
        {
        }

        private void display()
        {
            foreach (var item in carouselItems)
                panels.Add(new CarouselPanel(item));
        }

        internal class CarouselItem
        {
            public readonly object Model;

            /// <summary>
            /// The current drawable representation of this item, if it's currently being displayed.
            /// </summary>
            public Drawable? Drawable { get; set; }

            public readonly List<CarouselItem> Children = new List<CarouselItem>();

            public CarouselItem(object model)
            {
                Model = model;
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
                        Text = item?.ToString() ?? string.Empty,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }
        }
    }
}
