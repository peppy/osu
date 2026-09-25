// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class StatisticsGrid : CompositeDrawable
    {
        private StatisticsCell accuracyCell = null!;
        private StatisticsCell comboCell = null!;
        private StatisticsCell ppCell = null!;
        private GridContainer basicStatsFirstRow = null!;
        private GridContainer basicStatsSecondRow = null!;
        private GridContainer extendedStatsRow = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Shear = OsuGame.SHEAR,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children =
                [
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                accuracyCell = new StatisticsCell
                                {
                                    Caption = "Accuracy",
                                },
                                comboCell = new StatisticsCell
                                {
                                    Caption = "Combo",
                                },
                                ppCell = new StatisticsCell
                                {
                                    Caption = "PP",
                                }
                            },
                        },
                    },
                    basicStatsFirstRow = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    },
                    basicStatsSecondRow = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    },
                    extendedStatsRow = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = 10 },
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    },
                    new ModCell(),
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
            accuracyCell.Value = score.Value.Accuracy.FormatAccuracy();
            comboCell.Value = LocalisableString.Interpolate($"{score.Value.MaxCombo}x");
            ppCell.Value = score.Value.PP?.ToLocalisableString(@"N0") ?? "-";

            var hitStatistics = score.Value.GetStatisticsForDisplay().ToArray();
            var basicHitStatistics = hitStatistics.Where(s => s.Result.IsBasic()).ToArray();
            var otherHitStatistics = hitStatistics.Where(s => !s.Result.IsBasic()).ToArray();

            basicStatsFirstRow.Content = new[]
            {
                basicHitStatistics.Take(basicHitStatistics.Length > 4 ? basicHitStatistics.Length / 2 : 4).Select(s => new StatisticsCell
                {
                    Caption = s.DisplayName,
                    Value = s.Count.ToLocalisableString(@"N0"),
                    BaseFontSize = 32,
                    AccentColour = colours.ForHitResult(s.Result),
                }).ToArray<Drawable>(),
            };
            basicStatsSecondRow.Content = new[]
            {
                basicHitStatistics.Skip(basicHitStatistics.Length > 4 ? basicHitStatistics.Length / 2 : 4).Where(s => s.Result <= HitResult.Perfect).Select(s => new StatisticsCell
                {
                    Caption = s.DisplayName,
                    Value = s.Count.ToLocalisableString(@"N0"),
                    BaseFontSize = 32,
                    AccentColour = colours.ForHitResult(s.Result),
                }).ToArray<Drawable>(),
            };
            basicStatsSecondRow.Alpha = basicHitStatistics.Length > 4 ? 1 : 0;
            extendedStatsRow.Content = new[]
            {
                otherHitStatistics.Select(s => new StatisticsCell
                {
                    Caption = s.DisplayName,
                    Value = s.Count.ToLocalisableString(@"N0"),
                    MaxValue = s.MaxCount?.ToLocalisableString(@"N0"),
                    BaseFontSize = 28,
                }).ToArray<Drawable>(),
            };
        }

        public partial class StatisticsCell : CompositeDrawable
        {
            public required LocalisableString Caption { get; init; }

            private LocalisableString value;

            public LocalisableString Value
            {
                get => value;
                set
                {
                    this.value = value;

                    if (IsLoaded)
                        updateState();
                }
            }

            private LocalisableString? maxValue;

            public LocalisableString? MaxValue
            {
                get => value;
                set
                {
                    maxValue = value;

                    if (IsLoaded)
                        updateState();
                }
            }

            public float BaseFontSize { get; init; } = 36;

            public Colour4? AccentColour { get; init; }

            private Container contentContainer = null!;
            private Box backgroundBox = null!;
            private OsuSpriteText captionText = null!;
            private OsuSpriteText valueText = null!;
            private OsuSpriteText maxValueText = null!;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Shear = -OsuGame.SHEAR;
                Padding = new MarginPadding { Right = 5, };

                InternalChild = contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Children =
                    [
                        backgroundBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding
                            {
                                Horizontal = 7,
                                Vertical = 5,
                            },
                            Spacing = new Vector2(0, -3),
                            Children =
                            [
                                captionText = new OsuSpriteText
                                {
                                    Text = Caption.ToUpper(),
                                    Font = OsuFont.Default.With(size: BaseFontSize - 12, weight: FontWeight.Bold),
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(3),
                                    Children =
                                    [
                                        valueText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.Default.With(size: BaseFontSize),
                                            Colour = TotalScoreWedge.TEXT_GRADIENT,
                                        },
                                        maxValueText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.Default.With(size: BaseFontSize - 10),
                                            Margin = new MarginPadding { Bottom = 2 },
                                            Colour = TotalScoreWedge.TEXT_GRADIENT,
                                        },
                                    ],
                                },
                            ],
                        }
                    ]
                };

                if (AccentColour != null)
                {
                    backgroundBox.Colour = Interpolation.ValueAt(0.05, (Colour4)colourProvider.Background5, AccentColour.Value, 0, 1);
                    contentContainer.BorderThickness = 2;
                    contentContainer.BorderColour = AccentColour.Value;
                    captionText.Colour = AccentColour.Value;
                    valueText.Colour = AccentColour.Value;
                    maxValueText.Colour = AccentColour.Value;
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            private void updateState()
            {
                valueText.Text = value;
                maxValueText.Text = maxValue == null ? default : LocalisableString.Interpolate($"/ {maxValue}");
            }
        }

        // TODO: mark perfect results with green, maybe add a progress bar
        public partial class ModCell : CompositeDrawable
        {
            private ModDisplay modDisplay = null!;

            [Resolved]
            private IBindable<IScoreInfo> score { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                Shear = -OsuGame.SHEAR;
                Padding = new MarginPadding { Right = 5, };

                InternalChild = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding
                            {
                                Horizontal = 7,
                                Vertical = 5,
                            },
                            Spacing = new Vector2(0, -3),
                            Children =
                            [
                                new OsuSpriteText
                                {
                                    Text = @"Mods".ToUpperInvariant(),
                                    Font = OsuFont.Default.With(size: 16, weight: FontWeight.Bold),
                                },
                                modDisplay = new ModDisplay
                                {
                                    Scale = new Vector2(0.8f),
                                    ShowExtendedInformation = true,
                                    ExpansionMode = ExpansionMode.AlwaysExpanded
                                }
                            ],
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
                // TODO: needs to be unhacked
                modDisplay.Current.Value = (score.Value as ScoreInfo)?.Mods ?? [];
            }
        }
    }
}
