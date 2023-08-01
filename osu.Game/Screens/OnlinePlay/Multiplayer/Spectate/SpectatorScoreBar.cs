// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    internal partial class SpectatorScoreBar : CompositeDrawable
    {
        private readonly Room room;
        private readonly MultiplayerRoomUser[] users;
        private readonly PlayerArea[] instances;

        private Container content = null!;

        public SpectatorScoreBar(Room room, MultiplayerRoomUser[] users, PlayerArea[] instances)
        {
            this.room = room;
            this.users = users;
            this.instances = instances;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = SpectatorSongBar.HEIGHT;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray2,
                    RelativeSizeAxes = Axes.Both,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            // figure out how many teams we're dealing with
            var teams = users.GroupBy(u => (u.MatchState as TeamVersusUserState)?.TeamID ?? -1);

            if (teams.Count() == 2)
            {
                var team1 = teams.First().ToArray();
                var team2 = teams.Last().ToArray();

                var team1Leaderboard = new MultiSpectatorLeaderboard(team1)
                {
                    Margin = new MarginPadding { Top = 5 },
                };
                var team2Leaderboard = new MultiSpectatorLeaderboard(team2)
                {
                    Margin = new MarginPadding { Top = 5 },
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                };

                LoadComponentsAsync(new[]
                {
                    team1Leaderboard,
                    team2Leaderboard,
                }, d =>
                {
                    foreach (var instance in instances)
                    {
                        if (team1.Any(u => u.UserID == instance.UserId))
                            team1Leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);
                        else
                            team2Leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);
                    }

                    content.AddRange(d);

                    LoadComponentAsync(new MatchScoreDisplay
                    {
                        Team1Score = { BindTarget = team1Leaderboard.TeamScores.First().Value },
                        Team2Score = { BindTarget = team2Leaderboard.TeamScores.First().Value },
                    }, content.Add);
                });
            }
            else if (users.Length == 2)
            {
                var team1 = new[] { users[0] };
                var team2 = new[] { users[1] };

                var team1Leaderboard = new MultiSpectatorLeaderboard(team1)
                {
                    Margin = new MarginPadding { Top = 5 },
                };
                var team2Leaderboard = new MultiSpectatorLeaderboard(team2)
                {
                    Margin = new MarginPadding { Top = 5 },
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                };

                LoadComponentsAsync(new[]
                {
                    team1Leaderboard,
                    team2Leaderboard,
                }, d =>
                {
                    foreach (var instance in instances)
                    {
                        if (team1.Any(u => u.UserID == instance.UserId))
                            team1Leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);
                        else
                            team2Leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);
                    }

                    content.AddRange(d);

                    LoadComponentAsync(new MatchScoreDisplay
                    {
                        Team1Score = { BindTarget = team1Leaderboard.UserScores.First().Value.ScoreProcessor.TotalScore },
                        Team2Score = { BindTarget = team2Leaderboard.UserScores.First().Value.ScoreProcessor.TotalScore },
                    }, content.Add);
                });
            }
            else
            {
                MultiSpectatorLeaderboard leaderboard;

                LoadComponentsAsync(new[]
                {
                    leaderboard = new MultiSpectatorLeaderboard(users)
                    {
                        Margin = new MarginPadding { Top = 5 },
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    }
                }, d =>
                {
                    foreach (var instance in instances)
                        leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);

                    content.Add(leaderboard);
                });
            }
        }
    }
}
