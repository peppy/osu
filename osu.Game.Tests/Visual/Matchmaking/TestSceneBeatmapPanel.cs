// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public class TestSceneBeatmapPanel : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add beatmap panel", () =>
            {
                Child = new BeatmapPanel(new MultiplayerPlaylistItem { ID = 1, StarRating = 5.3 })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });
        }

        [Test]
        public void TestAddRemoveSelection()
        {
            AddStep("toggle peppy selection", () => MultiplayerClient.MatchmakingToggleUserSelection(2, 1).WaitSafely());
            AddStep("toggle flyte selection", () => MultiplayerClient.MatchmakingToggleUserSelection(3103765, 1).WaitSafely());

            AddStep("toggle peppy selection", () => MultiplayerClient.MatchmakingToggleUserSelection(2, 1).WaitSafely());
            AddStep("toggle flyte selection", () => MultiplayerClient.MatchmakingToggleUserSelection(3103765, 1).WaitSafely());
        }
    }
}
