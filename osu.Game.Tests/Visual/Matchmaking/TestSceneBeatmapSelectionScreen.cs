// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectionScreen : ScreenTestScene
    {
        [Test]
        public void TestBeatmapSelectionScreen()
        {
            BeatmapSelectionScreen screen = null!;

            IReadOnlyList<APIUser> users = new[]
            {
                new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                },
                new APIUser
                {
                    Id = 1040328,
                    Username = "smoogipoo",
                },
                new APIUser
                {
                    Id = 6573093,
                    Username = "OliBomby",
                },
                new APIUser
                {
                    Id = 7782553,
                    Username = "aesth",
                },
                new APIUser
                {
                    Id = 6411631,
                    Username = "Maarvin",
                }
            };

            AddStep("push screen", () => LoadScreen(screen = new BeatmapSelectionScreen()));
            AddStep("add user selections", () => screen.DistributeUsers(users));
            AddStep("hide panels", () => screen.HidePanels(4));
            AddStep("show final beatmap", () => screen.SelectFinalBeatmap());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void TestNumberBeatmapsRemaining(int remaining)
        {
            BeatmapSelectionScreen screen;

            AddStep("push screen", () =>
            {
                LoadScreen(screen = new BeatmapSelectionScreen());
                Scheduler.AddDelayed(() => screen.HidePanels(remaining), 250);
            });
        }
    }
}
