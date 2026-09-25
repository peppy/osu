// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class TotalScoreWedge : CompositeDrawable
    {
        public static readonly ColourInfo TEXT_GRADIENT = ColourInfo.GradientVertical(Colour4.White, Colour4.FromHex(@"B2E5FE"));

        private Sprite perfectIndicator = null!;
        private OsuSpriteText totalScoreText = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        private Bindable<ScoringMode> scoringMode { get; set; } = new Bindable<ScoringMode>();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures, OsuConfigManager configManager)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren =
            [
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Shear = OsuGame.SHEAR,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Masking = true,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5.Opacity(0.95f),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding
                            {
                                Vertical = 15,
                                Right = 30,
                            },
                            Spacing = new Vector2(20),
                            Shear = -OsuGame.SHEAR,
                            // TODO: this layout is a hackjob and probably not going to hold up to scrutiny.
                            // think about how to make it better later
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children =
                            [
                                perfectIndicator = new Sprite
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Texture = textures.Get(@"Icons/Ranking/perfect"),
                                    Size = new Vector2(65),
                                    AlwaysPresent = true,
                                    Colour = new ColourInfo
                                    {
                                        TopLeft = Colour4.FromHex(@"00FFAA"),
                                        TopRight = Colour4.FromHex(@"7CF6FF"),
                                        BottomLeft = Colour4.FromHex(@"7CF6FF"),
                                        BottomRight = Colour4.FromHex(@"FF9AD7"),
                                    }
                                },
                                totalScoreText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    // TODO: classic scoring likely breaks this sizing. figure out later what to do with ultra large score numbers
                                    Font = OsuFont.TorusAlternate.With(size: 120, weight: FontWeight.Light, fixedWidth: true),
                                    Spacing = new Vector2(-5),
                                    Colour = TEXT_GRADIENT,
                                    UseFullGlyphHeight = false,
                                    Margin = new MarginPadding { Top = 5, }, // `UseFullGlyphHeight` *almost* does the job to trim the glyph paddings, but it still can look offset because of decimal commas and such
                                },
                            ]
                        }
                    ]
                },
                // TODO: not actually hooked up to anything because this doesn't exist on old screens
                // figure it out later
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    RelativeAnchorPosition = new Vector2(0.9f, 0),
                    Shear = OsuGame.SHEAR,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Masking = true,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Colour4.FromHex(@"#FFE7A8"), Colour4.FromHex(@"#FFB800")),
                        },
                        new OsuSpriteText
                        {
                            Colour = colourProvider.Background5,
                            Text = "PERSONAL BEST",
                            UseFullGlyphHeight = false,
                            Spacing = new Vector2(1.5f),
                            Font = OsuFont.Style.Body.With(weight: FontWeight.Bold),
                            Shear = -OsuGame.SHEAR,
                            Margin = new MarginPadding
                            {
                                Horizontal = 12,
                                Vertical = 6,
                            }
                        }
                    ]
                }
            ];

            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState());
            scoringMode.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            totalScoreText.Text = score.Value.GetDisplayScore(scoringMode.Value).ToLocalisableString(@"N0");
            perfectIndicator.Alpha = score.Value.MaxCombo == score.Value.GetMaximumAchievableCombo() ? 1 : 0;
        }
    }
}
