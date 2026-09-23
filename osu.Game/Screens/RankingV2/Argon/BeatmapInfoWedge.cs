// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    // TODO: transition / animation pass
    public partial class BeatmapInfoWedge : CompositeDrawable
    {
        public const float SUB_WEDGE_HEIGHT = 35;

        private const float text_padding = 8 + ShearedButton.CORNER_RADIUS;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private MarqueeContainer titleText = null!;
        private MarqueeContainer artistText = null!;
        private Container rulesetIconContainer = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private OsuTextFlowContainer difficultyText = null!;

        private CancellationTokenSource? difficultyRetrievalCancellation;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren =
            [
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Shear = OsuGame.SHEAR,
                    Children =
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Radius = 2,
                                Offset = new Vector2(0, 1),
                                Colour = Colour4.Black.Opacity(0.25f),
                                Hollow = true,
                            },
                            CornerRadius = ShearedButton.CORNER_RADIUS,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background3.Opacity(0.9f),
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding
                                    {
                                        Left = text_padding,
                                        Right = 30,
                                        Top = 15 + ShearedButton.CORNER_RADIUS,
                                        Bottom = 10 + SUB_WEDGE_HEIGHT / 2,
                                    },
                                    Shear = -OsuGame.SHEAR,
                                    Children = new Drawable[]
                                    {
                                        statusPill = new BeatmapSetOnlineStatusPill(),
                                        titleText = new MarqueeContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        artistText = new MarqueeContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
                                    }
                                },
                            ]
                        },
                        new ShearAligningWrapper(new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = SUB_WEDGE_HEIGHT,
                            Origin = Anchor.CentreLeft,
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
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Shear = -OsuGame.SHEAR,
                                    Spacing = new Vector2(5),
                                    Padding = new MarginPadding { Horizontal = text_padding, },
                                    Children =
                                    [
                                        rulesetIconContainer = new Container
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        starRatingDisplay = new StarRatingDisplay(new StarDifficulty())
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        // TODO: probably overflows when text is long enough
                                        difficultyText = new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Heading2)
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        }
                                    ]
                                }
                            ],
                        })
                    ],
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            Debug.Assert(score.Value.Beatmap != null);

            statusPill.Status = score.Value.Beatmap.Status;
            titleText.CreateContent = () => new OsuSpriteText
            {
                Text = new RomanisableString(score.Value.Beatmap.Metadata.TitleUnicode, score.Value.Beatmap.Metadata.Title),
                Font = OsuFont.Style.Title,
            };
            artistText.CreateContent = () => new OsuSpriteText
            {
                Text = new RomanisableString(score.Value.Beatmap.Metadata.ArtistUnicode, score.Value.Beatmap.Metadata.Artist),
                Font = OsuFont.Style.Heading2,
            };

            rulesetIconContainer.Clear();
            rulesetIconContainer.Add(score.Value.Ruleset.CreateInstance().CreateIcon().With(i => i.Size = new Vector2(20)));
            difficultyText.Clear();
            difficultyText.AddText(score.Value.Beatmap.DifficultyName, t => t.Font = OsuFont.Style.Heading2);
            difficultyText.AddText(" mapped by ", t => t.Font = OsuFont.Style.Caption1);
            difficultyText.AddText(score.Value.Beatmap.Metadata.Author.Username, t => t.Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold));

            difficultyRetrievalCancellation?.Cancel();
            difficultyRetrievalCancellation = new CancellationTokenSource();
            // TODO: not passing mods here because it's annoying to put it in `IScoreInfo` without an `IConfiguredMod` interface or similar
            difficultyCache.GetDifficultyAsync(score.Value.Beatmap, score.Value.Ruleset, cancellationToken: difficultyRetrievalCancellation.Token)
                           .ContinueWith(t =>
                           {
                               var difficulty = t.GetResultSafely() ?? new StarDifficulty(score.Value.Beatmap.StarRating, 0);
                               Schedule(() =>
                               {
                                   starRatingDisplay.Current.Value = difficulty;

                                   var col = starRatingDisplay.GetForegroundColourForAssociatedControls();
                                   rulesetIconContainer.Colour = col;
                                   difficultyText.Colour = col;
                               });
                           });
        }
    }
}
