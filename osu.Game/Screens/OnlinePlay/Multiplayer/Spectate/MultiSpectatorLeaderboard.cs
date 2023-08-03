// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class MultiSpectatorLeaderboard : MultiplayerGameplayLeaderboard
    {
        private const float panel_height = 16;

        public MultiSpectatorLeaderboard(MultiplayerRoomUser[] users)
            : base(users)
        {
        }

        public void AddClock(int userId, IClock clock)
        {
            if (!UserScores.TryGetValue(userId, out var data))
                throw new ArgumentException(@"Provided user is not tracked by this leaderboard", nameof(userId));

            data.ScoreProcessor.ReferenceClock = clock;
        }

        protected override float PanelHeight => panel_height;

        protected override GameplayLeaderboardScore CreateLeaderboardScoreDrawable(IUser? user, bool isTracked) =>
            new SpectatorLeaderboardScore(user, isTracked);

        protected override void Update()
        {
            base.Update();

            foreach (var (_, data) in UserScores)
                data.ScoreProcessor.UpdateScore();
        }

        public partial class SpectatorLeaderboardScore : GameplayLeaderboardScore
        {
            private OsuSpriteText scoreText = null!;
            private OsuSpriteText accuracyText = null!;
            private OsuSpriteText comboText = null!;

            public SpectatorLeaderboardScore(IUser? user, bool isTracked)
                : base(user, isTracked)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.7f,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(3),
                        Children = new Drawable[]
                        {
                            scoreText = new OsuSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.4f,

                                Spacing = new Vector2(-1f, 0f),
                                Font = OsuFont.Torus.With(size: panel_height, weight: FontWeight.SemiBold, fixedWidth: true),
                            },
                            accuracyText = new OsuSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.3f,
                                Font = OsuFont.Torus.With(size: panel_height, weight: FontWeight.SemiBold, fixedWidth: true),
                                Spacing = new Vector2(-1f, 0f),
                                Shadow = false,
                            },
                            comboText = new OsuSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.3f,
                                Spacing = new Vector2(-1f, 0f),
                                Font = OsuFont.Torus.With(size: panel_height, weight: FontWeight.SemiBold, fixedWidth: true),
                                Shadow = false,
                            },
                        }
                    },
                    new TruncatingSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.3f,
                        Font = OsuFont.Torus.With(size: panel_height, weight: FontWeight.SemiBold),
                        Text = User?.Username ?? string.Empty,
                    }
                };

                Accuracy.BindValueChanged(v => accuracyText.Text = v.NewValue.FormatAccuracy(), true);
                Combo.BindValueChanged(v => comboText.Text = $"{v.NewValue}x", true);
            }

            protected override void UpdateScore(string scoreString) => scoreText.Text = scoreString;
        }
    }
}
