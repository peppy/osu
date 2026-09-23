// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    public partial class LegacyRankingPanel : CompositeDrawable
    {
        private Vector2 baselinePosition;

        private LegacySpriteText scoreText = null!;
        private Container<LegacyRankingElement> rulesetRankingElements = null!;
        private LegacyRankingElement maxComboElement = null!;
        private LegacyRankingElement accuracyElement = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();

        private const float textx1 = 80;
        private const float imgx1 = 40;
        private const float textx2 = 280;
        private const float imgx2 = 240;

        private const float row1 = 160;
        private const float row2 = 220;
        private const float row3 = 280;
        private const float row4 = 320;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuConfigManager config)
        {
            AutoSizeAxes = Axes.Both;

            // TODO: move to skinnable container defaults
            bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;
            baselinePosition = new Vector2(0, useNewLayout ? 64 : 46);
            Position = baselinePosition * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            int row4Offset = useNewLayout ? 20 : 0;

            InternalChildren =
            [
                new Sprite
                {
                    Texture = skin.GetTexture(@"ranking-panel"),
                },
                scoreText = new LegacySpriteText(LegacyFont.Score)
                {
                    Text = "0419611",
                    Origin = Anchor.Centre,
                    // (220, 94) - position of `LegacyRankingPanel` itself
                    Position = (new Vector2(220, 94) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Scale = new Vector2(useNewLayout ? 1.3f : 1.05f),
                    // TODO: there's stuff done with overlap here, cross-check it
                    FixedWidth = true,
                },
                rulesetRankingElements = new Container<LegacyRankingElement>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                maxComboElement = new LegacyRankingElement
                {
                    ElementName = @"ranking-maxcombo",
                    Origin = Anchor.TopLeft,
                    Position = (new Vector2(imgx1 - 35, row4 - row4Offset) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    ScoreTextPosition = (new Vector2(textx1 - 65, row4 + 10) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
                accuracyElement = new LegacyRankingElement
                {
                    ElementName = @"ranking-accuracy",
                    Origin = Anchor.TopLeft,
                    Position = (new Vector2(imgx2 - 58, row4 - row4Offset) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    ScoreTextPosition = (new Vector2(textx2 - 86, row4 + 10) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
            ];

            config.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState());
            scoringMode.BindValueChanged( _ => updateState(), true);
        }

        private void updateState()
        {
            long totalScore = score.Value.GetDisplayScore(scoringMode.Value);
            string template = new string(Enumerable.Repeat('0', scoringMode.Value == ScoringMode.Standardised ? 7 : 8).ToArray());
            scoreText.Text = totalScore.ToString(template);

            rulesetRankingElements.Clear();

            switch (score.Value.Ruleset.OnlineID)
            {
                case 0:
                    rulesetRankingElements.AddRange([
                        new LegacyRankingElement
                        {
                            ElementName = @"hit300",
                            ScoreText = $"{score.Value.GetCount300()}x",
                            Position = (new Vector2(imgx1, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"hit100",
                            ScoreText = $"{score.Value.GetCount100()}x",
                            Position = (new Vector2(imgx1, row2) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"hit50",
                            ScoreText = $"{score.Value.GetCount50()}x",
                            Position = (new Vector2(imgx1, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"hit0",
                            ScoreText = $"{score.Value.GetCountMiss()}x",
                            Position = (new Vector2(imgx2, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                    ]);
                    break;

                // TODO the rest
                case 1:
                    break;

                case 2:
                    break;

                case 3:
                    break;

                // TODO: good luck with custom rulesets!!!
            }

            maxComboElement.ScoreText = $@"{score.Value.MaxCombo}x";
            accuracyElement.ScoreText = $@"{score.Value.Accuracy * 100:0.00}%"; // TODO: probably has rounding shit issues
        }

        public partial class LegacyRankingElement : CompositeDrawable
        {
            public required string ElementName { get; init; }

            private string? scoreText;

            public string? ScoreText
            {
                get => scoreText;
                set
                {
                    scoreText = value;
                    if (IsLoaded)
                        updateState();
                }
            }

            public new Anchor Origin { get; init; } = Anchor.Centre;

            public Vector2? ScoreTextPosition { get; init; }

            private LegacySpriteText text = null!;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                AutoSizeAxes = Axes.Both;
                bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;

                AddInternal(new Sprite
                {
                    Texture = skin.GetTexture(ElementName),
                    Anchor = Anchor.TopLeft,
                    Origin = Origin,
                });

                AddInternal(text = new LegacySpriteText(LegacyFont.Score)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Scale = new Vector2(1.12f),
                });

                if (ScoreTextPosition != null)
                    text.Position = ScoreTextPosition.Value - Position;
                else
                    text.Position = new Vector2(40, useNewLayout ? -16 : -25) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            private void updateState()
            {
                text.Alpha = ScoreText != null ? 1 : 0;
                if (ScoreText != null)
                    text.Text = ScoreText;
            }
        }
    }
}
