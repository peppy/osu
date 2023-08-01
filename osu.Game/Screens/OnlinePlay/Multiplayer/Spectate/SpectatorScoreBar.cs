// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    internal partial class SpectatorScoreBar : CompositeDrawable
    {
        private readonly Room room;
        private readonly MultiplayerRoomUser[] users;
        private readonly PlayerArea[] instances;

        private FillFlowContainer flow = null!;
        private MultiSpectatorLeaderboard leaderboard = null!;

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
                    Colour = colours.Blue4,
                    RelativeSizeAxes = Axes.Both,
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                }
            };

            LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(users), _ =>
            {
                foreach (var instance in instances)
                    leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);

                flow.Add(leaderboard);

                if (leaderboard.TeamScores.Count == 2)
                {
                    LoadComponentAsync(new MatchScoreDisplay
                    {
                        Team1Score = { BindTarget = leaderboard.TeamScores.First().Value },
                        Team2Score = { BindTarget = leaderboard.TeamScores.Last().Value },
                    }, d => flow.Insert(-1, d));
                }
            });
        }
    }
}
