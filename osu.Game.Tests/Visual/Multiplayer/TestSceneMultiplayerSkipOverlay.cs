// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerSkipOverlay : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom()));
            WaitForJoined();

            AddStep("add skip overlay", () =>
            {
                GameplayClockContainer gameplayClockContainer;

                var working = CreateWorkingBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));

                Child = gameplayClockContainer = new MasterGameplayClockContainer(working, 0)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new MultiplayerSkipOverlay(120000)
                    },
                };

                gameplayClockContainer.Start();
            });

            AddStep("set playing state", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Playing));
        }

        [Test]
        public void TestSkip()
        {
            for (int i = 0; i < 4; i++)
            {
                int userId = i;

                AddStep($"join user {userId}", () =>
                {
                    MultiplayerClient.AddUser(new APIUser
                    {
                        Id = userId,
                        Username = $"User {userId}"
                    });

                    MultiplayerClient.ChangeUserState(userId, MultiplayerUserState.Playing);
                });
            }

            for (int i = 0; i < 2; i++)
            {
                int userId = i;
                AddStep($"user {userId} votes", () => MultiplayerClient.UserVoteToSkipIntro(userId).WaitSafely());
            }

            AddStep("local user votes", () => MultiplayerClient.VoteToSkipIntro().WaitSafely());

        }
    }
}
