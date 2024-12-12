// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2 : OsuManualInputManagerTestScene
    {
        private readonly BindableList<BeatmapSetInfo> beatmaps = new BindableList<BeatmapSetInfo>();

        [Cached(typeof(BeatmapStore))]
        private BeatmapStore store;

        private OsuTextFlowContainer stats = null!;
        private BeatmapCarouselV2 carousel = null!;

        public TestSceneBeatmapCarouselV2()
        {
            store = new TestBeatmapStore
            {
                BeatmapSets = { BindTarget = beatmaps }
            };

            Scheduler.AddDelayed(updateStats, 100, true);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create components", () =>
            {
                beatmaps.Clear();

                Children = new Drawable[]
                {
                    stats = new OsuTextFlowContainer
                    {
                        Padding = new MarginPadding(10),
                    },
                    carousel = new BeatmapCarouselV2
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(500, 700),
                    },
                };
            });
            AddStep("add beatmaps", () =>
            {
                for (int i = 0; i < 10; i++)
                    beatmaps.Add(TestResources.CreateTestBeatmapSetInfo());
            });
        }

        [Test]
        public void TestBasic()
        {
        }

        private void updateStats()
        {
            if (carousel.IsNull())
                return;

            stats.Text = $"""
                                        store
                                          sets: {beatmaps.Count}
                                          beatmaps: {beatmaps.Sum(s => s.Beatmaps.Count)}
                                        carousel:
                                          sorting: {carousel.IsSorting}
                                          tracked: {carousel.BeatmapsTracked}
                                          displayable: {carousel.DisplayableItems}
                                          displayed: {carousel.VisibleItems}
                          """;
        }
    }
}
