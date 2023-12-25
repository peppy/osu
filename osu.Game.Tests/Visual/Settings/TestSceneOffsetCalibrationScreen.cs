// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Screens.Utility;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneOffsetCalibrationScreen : ScreenTestScene
    {
        private OffsetCalibrationScreen latencyCertifier = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Load screen", () => LoadScreen(latencyCertifier = new OffsetCalibrationScreen()));
            AddUntilStep("wait for load", () => latencyCertifier.IsLoaded);
        }

        [Test]
        public void TestSimple()
        {
        }
    }
}
