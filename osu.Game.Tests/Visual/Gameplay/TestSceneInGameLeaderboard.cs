﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneInGameLeaderboard : OsuTestScene
    {
        private readonly TestInGameLeaderboard ingameLeaderboard;

        public TestSceneInGameLeaderboard()
        {
            Add(ingameLeaderboard = new TestInGameLeaderboard
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(1.5f),
                RelativeSizeAxes = Axes.X,
                PlayerUser = new APIUser { Username = "You" },
                Leaderboard = new TestLeaderboard(new[]
                {
                    new ScoreInfo { User = new APIUser { Username = "Player 1" }, TotalScore = 3984294 },
                    new ScoreInfo { User = new APIUser { Username = "Player 2" }, TotalScore = 3857439 },
                    new ScoreInfo { User = new APIUser { Username = "Player 3" }, TotalScore = 3102893 },
                    new ScoreInfo { User = new APIUser { Username = "Player 4" }, TotalScore = 2943828 },
                    new ScoreInfo { User = new APIUser { Username = "Player 5" }, TotalScore = 2712839 },
                    new ScoreInfo { User = new APIUser { Username = "Player 6" }, TotalScore = 2459483 },
                    new ScoreInfo { User = new APIUser { Username = "Player 7" }, TotalScore = 2213902 },
                    new ScoreInfo { User = new APIUser { Username = "Player 8" }, TotalScore = 2029384 },
                }),
            });
        }

        [Test]
        public void TestMoveUserScore()
        {
            moveUserScore("Player 7", 7, false);
            moveUserScore("Player 4", 4, false);
            //moveUserScore("Player 8", 9, true);
            moveUserScore("Player 1", 2, true);
        }

        /// <summary>
        /// Moves user score below or above the target.
        /// </summary>
        /// <param name="target">The target user to go above it or below it</param>
        /// <param name="yourExpectedPosition">The expected new position of the user (you)</param>
        /// <param name="below">Whether the user will go below the target or above it</param>
        private void moveUserScore(string target, int yourExpectedPosition, bool below)
        {
            long scoreID = ingameLeaderboard.GetScoreByUsername(target);

            AddAssert("is target present", () => scoreID > 0);
            AddStep($"set user score {(below ? "below" : "above")} {target}", () => ingameLeaderboard.UserTotalScore.Value = scoreID + (below ? -1234 : 1234));
            AddUntilStep($"is user score position #{yourExpectedPosition}", () => ingameLeaderboard.CheckUserPosition(yourExpectedPosition));
            AddAssert("is scores count correct", () => ingameLeaderboard.IsScoresCountCorrect);
        }

        private class TestInGameLeaderboard : InGameLeaderboard
        {
            public bool IsScoresCountCorrect => ScoresContainer.Count == 7;

            public bool CheckUserPosition(int position) => UserScoreItem.ScorePosition == position;

            public long GetScoreByUsername(string user)
            {
                foreach (var score in Leaderboard.Scores)
                {
                    if (score.User.Username == user)
                        return score.TotalScore;
                }

                return 0;
            }
        }

        private class TestLeaderboard : ILeaderboard
        {
            public IEnumerable<ScoreInfo> Scores { get; set; }

            public bool IsOnlineScope => false;

            public TestLeaderboard(IEnumerable<ScoreInfo> scores)
            {
                Scores = scores;
            }

            public void RefreshScores()
            {
            }
        }
    }
}
