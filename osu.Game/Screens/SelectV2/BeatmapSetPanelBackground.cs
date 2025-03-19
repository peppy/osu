// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanelBackground : CompositeDrawable
    {
        private Sprite sprite = null!;

        private static readonly GlobalStatistic<int> stat_texture_loads;
        private static readonly GlobalStatistic<int> stat_texture_sets;

        private static readonly ThreadedTaskScheduler background_loader_scheduler;

        static BeatmapSetPanelBackground()
        {
            stat_texture_loads = GlobalStatistics.Get<int>("Carousel", "Texture loads");
            stat_texture_sets = GlobalStatistics.Get<int>("Carousel", "Texture sets");

            background_loader_scheduler = new ThreadedTaskScheduler(1, "background loader");
        }

        private WorkingBeatmap? working;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public WorkingBeatmap? Beatmap
        {
            set
            {
                working = value;
                updateBeatmap();
            }
        }

        [Resolved]
        private GameHost host { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                sprite = new Sprite(),
                new FillFlowContainer
                {
                    Depth = -1,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                    Shear = new Vector2(0.8f, 0),
                    Alpha = 0.5f,
                    Children = new[]
                    {
                        // The left half with no gradient applied
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Width = 0.4f,
                        },
                        // Piecewise-linear gradient with 3 segments to make it appear smoother
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                            Width = 0.05f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                            Width = 0.2f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                            Width = 0.05f,
                        },
                    }
                },
            };

            updateBeatmap();
        }

        private void updateBeatmap()
        {
            if (LoadState == LoadState.NotLoaded)
                return;

            sprite.Texture = null;
            sprite.Hide();

            if (working == null)
                return;

            // In local testing, this never really helps.. but maybe it will on slower devices?
            var localCts = new CancellationTokenSource();

            cts.Cancel();
            cts = localCts;

            Task.Factory.StartNew(() =>
            {
                var panelBackground = working?.GetPanelBackground();
                stat_texture_loads.Value++;

                if (localCts.IsCancellationRequested)
                    return;

                // This is used to perform the texture load and transform while the drawable is potentially still off-screen.
                // Scheduling locally won't work because of masking checks.
                host.UpdateThread.Scheduler.Add(() =>
                {
                    if (localCts.IsCancellationRequested)
                        return;

                    stat_texture_sets.Value++;

                    sprite.FadeInFromZero(1000, Easing.OutQuint);
                    sprite.Texture = panelBackground;
                });
            }, localCts.Token, TaskCreationOptions.HideScheduler, background_loader_scheduler);
        }
    }
}
