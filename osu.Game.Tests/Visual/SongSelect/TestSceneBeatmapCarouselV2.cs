// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2 : OsuManualInputManagerTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create carousel", () => Child = new BeatmapCarouselV2());
        }

        [Test]
        public void TestBasic()
        {
        }
    }
}
