// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselUpdateHandling : BeatmapCarouselTestScene
    {
        [Test]
        public void TestBeatmapUpdated()
        {
            CreateCarousel();
            AddBeatmaps(10, 3);

            AddStep("update beatmap with same reference", () =>
            {
                var newBeatmap = BeatmapSets.ElementAt(5);
                BeatmapSets.ReplaceRange(5, 1, [newBeatmap]);
            });
        }
    }
}
