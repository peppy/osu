// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public class TestSceneMatchmakingQueueBanner : MultiplayerTestScene
    {
        private readonly Mock<TestPerformerFromScreenRunner> performer = new Mock<TestPerformerFromScreenRunner>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs<IPerformFromScreenRunner>(performer.Object);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add banner", () => Child = new MatchmakingQueueBanner
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestChangeState()
        {
            AddStep("out of queue", () => ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(null));

            AddStep("in queue (1/8)", () => ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(new MatchmakingQueueStatus.InQueue
            {
                PlayerCount = 1,
                RoomSize = 8
            }));

            AddStep("in queue (4/8)", () => ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(new MatchmakingQueueStatus.InQueue
            {
                PlayerCount = 4,
                RoomSize = 8
            }));

            AddStep("match found", () =>
            {
                Room room;

                MultiplayerClient.AddServerSideRoom(room = new Room
                {
                    Name = "Test Room",
                    Playlist =
                    [
                        new PlaylistItem(CreateAPIBeatmap())
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                        }
                    ]
                }, API.LocalUser.Value);

                ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(new MatchmakingQueueStatus.FoundMatch
                {
                    RoomId = room.RoomID!.Value
                });
            });

            AddStep("out of queue", () => ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(null));
        }

        // interface mocks break hot reload, mocking this stub implementation instead works around it.
        // see: https://github.com/moq/moq4/issues/1252
        [UsedImplicitly]
        public class TestPerformerFromScreenRunner : IPerformFromScreenRunner
        {
            public virtual void PerformFromScreen(Action<IScreen> action, IEnumerable<Type>? validScreens = null)
            {
            }
        }
    }
}
