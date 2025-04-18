// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselUpdateHandling : BeatmapCarouselTestScene
    {
        private BeatmapSetInfo baseTestBeatmap = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
            AddBeatmaps(5, 3);
            AddStep("generate and add test beatmap", () =>
            {
                baseTestBeatmap = TestResources.CreateTestBeatmapSetInfo();

                var metadata = new BeatmapMetadata
                {
                    Artist = "update test",
                    Title = "beatmap",
                };

                foreach (var b in baseTestBeatmap.Beatmaps)
                    b.Metadata = metadata;
                BeatmapSets.Add(baseTestBeatmap);
            });
        }

        [Test]
        public void TestBeatmapSetUpdatedNoop()
        {
            AddStep("update beatmap with same reference", () =>
            {
                BeatmapSets.ReplaceRange(5, 1, [baseTestBeatmap]);
            });
        }

        [Test]
        public void TestBeatmapSetMetadataUpdated()
        {
            AddStep("update beatmap with different reference", () =>
            {
                var updatedSet = new BeatmapSetInfo(baseTestBeatmap.Beatmaps)
                {
                    ID = baseTestBeatmap.ID,
                    OnlineID = baseTestBeatmap.OnlineID,
                    DateAdded = baseTestBeatmap.DateAdded,
                    DateSubmitted = baseTestBeatmap.DateSubmitted,
                    DateRanked = baseTestBeatmap.DateRanked,
                    Status = baseTestBeatmap.Status,
                    StatusInt = baseTestBeatmap.StatusInt,
                    DeletePending = baseTestBeatmap.DeletePending,
                    Hash = baseTestBeatmap.Hash,
                    Protected = baseTestBeatmap.Protected,
                };

                int originalIndex = BeatmapSets.IndexOf(baseTestBeatmap);

                BeatmapSets.ReplaceRange(originalIndex, 1, [updatedSet]);
            });
        }
    }
}
