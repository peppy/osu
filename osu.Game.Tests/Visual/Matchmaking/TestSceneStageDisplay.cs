// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneStageDisplay : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom()));
            WaitForJoined();

            AddStep("add display", () => Child = new StageDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
            });
        }

        [Test]
        public void TestStartCountdown()
        {
            addStage(MatchmakingStage.WaitingForClientsJoin, 0);

            for (int i = 0; i < 5; i++)
            {
                addStage(MatchmakingStage.RoundWarmupTime, i);
                addStage(MatchmakingStage.UserBeatmapSelect, i);
                addStage(MatchmakingStage.ServerBeatmapFinalised, i);
                addStage(MatchmakingStage.WaitingForClientsBeatmapDownload, i);
                addStage(MatchmakingStage.GameplayWarmupTime, i);
                addStage(MatchmakingStage.Gameplay, i);
                addStage(MatchmakingStage.ResultsDisplaying, i);
            }

            addStage(MatchmakingStage.Ended, 4);
        }

        private void addStage(MatchmakingStage status, int round)
        {
            AddStep($"{status} ({round})", () =>
            {
                MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
                {
                    Stage = status,
                    CurrentRound = round,
                }).WaitSafely();

                MultiplayerClient.StartCountdown(new MatchmakingStageCountdown
                {
                    Stage = status,
                    TimeRemaining = TimeSpan.FromSeconds(5)
                }).WaitSafely();
            });

            AddWaitStep("wait a bit", 10);
        }
    }
}
