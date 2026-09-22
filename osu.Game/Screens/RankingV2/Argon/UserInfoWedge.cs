// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class UserInfoWedge : CompositeDrawable
    {
        private const float height = 60;
        private const float spacing = 10;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        private OsuSpriteText positionText = null!;
        private UpdateableAvatar userAvatar = null!;
        private UserCoverBackground userCover = null!;
        private OsuSpriteText usernameText = null!;
        private OsuSpriteText achievedOnText = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Shear = OsuGame.SHEAR,
                CornerRadius = ShearedButton.CORNER_RADIUS,
                Masking = true,
                Children =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions =
                        [
                            new Dimension(GridSizeMode.AutoSize, minSize: 150),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension()
                        ],
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                positionText = new OsuSpriteText
                                {
                                    Text = "#1234",
                                    Font = OsuFont.Style.Heading2,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Shear = -OsuGame.SHEAR,
                                    Margin = new MarginPadding { Right = spacing, },
                                },
                                new Container
                                {
                                    Size = new Vector2(height),
                                    Masking = true,
                                    CornerRadius = ShearedButton.CORNER_RADIUS,
                                    Child = userAvatar = new UpdateableAvatar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Shear = -OsuGame.SHEAR,
                                        Scale = new Vector2(1.15f), // scaled up to cover sides which have extra space due to the shearing
                                    },
                                    Depth = float.MinValue, // set so that this is in front of the next cell, which has the user cover
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children =
                                    [
                                        userCover = new UserCoverBackground
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding
                                            {
                                                Left = -ShearedButton.CORNER_RADIUS, // negative padding so that the cover can underlap the user avatar where the rounded corners are
                                            },
                                            Alpha = 0.25f,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Shear = -OsuGame.SHEAR,
                                            Padding = new MarginPadding { Left = spacing, },
                                            Children =
                                            [
                                                usernameText = new OsuSpriteText
                                                {
                                                    Text = "joemama",
                                                    Font = OsuFont.Style.Heading2
                                                },
                                                achievedOnText = new OsuSpriteText
                                                {
                                                    Text = "Achieved on 2025/02/03 12:34",
                                                    Font = OsuFont.Style.Body,
                                                    Colour = colourProvider.Content2,
                                                }
                                            ]
                                        }
                                    ],
                                },
                            }
                        }
                    }
                ]
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            positionText.Alpha = score.Value.Position != null ? 1 : 0;
            positionText.Text = score.Value.Position?.FormatRank().Insert(0, "#") ?? "";

            // TODO: this will probably not fly in the long run but this is too hard to fix for now as the avatar situation stinks. revisit later
            // this probably needs to do an actual user lookup if this soft cast fails
            if (score.Value.User is APIUser apiUser)
            {
                userAvatar.User = apiUser;
                userCover.User = apiUser;
            }

            usernameText.Text = score.Value.User.Username;
            achievedOnText.Text = $"Achieved on {score.Value.Date:g}";
        }
    }
}
