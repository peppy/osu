// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapMetadataUpdates : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Test]
        public void TestUpdateAfterLocalModifications()
        {
        }
    }
}
