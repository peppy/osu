// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    /*
     * TODO:
     * - Skinnability is probably not going to work via substituting screen implementations.
     *   That is temporary, this is being done via full screens for now just to get a first pass at the layout.
     * - Every single usage of `useNewLayout`-like checks that was ported is suspicious.
     *   Needs testing on pre-v2 skins.
     */
    public partial class LegacyResultsScreenV2 : ScreenWithBeatmapBackground
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        // TODO: multiplayer screens accept null score when showing a playlist item's scores - decide how to handle that
        public LegacyResultsScreenV2(IScoreInfo initialScore)
        {
            score.Value = initialScore;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                new LegacyRankingBackgroundOverlay(),
                new BoxElement
                {
                    Width = 9999,
                    Height = 60 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    CornerRadius = { Value = 0, },
                    AccentColour = { Value = Colour4.Black, },
                },
                new BeatmapAttributeText
                {
                    Scale = new Vector2(22 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR / BeatmapAttributeText.DEFAULT_TEXT_SIZE),
                    Template = { Value = @"{Artist} - {Title} [{DifficultyName}]" }
                },
                new BeatmapAttributeText
                {
                    Position = new Vector2(1, 20) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Scale = new Vector2(16 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR / BeatmapAttributeText.DEFAULT_TEXT_SIZE),
                    Template = { Value = @"Beatmap by {Creator}" } // TODO: localisation...???
                },
                new ScoreAttributeText
                {
                    Position = new Vector2(1, 34) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Scale = new Vector2(16 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR / BeatmapAttributeText.DEFAULT_TEXT_SIZE),
                    Template = { Value = @"Played by {Username} on {Date}" } // TODO: localisation...???
                },
                new SkinnableSprite
                {
                    SpriteName = { Value = @"ranking-title" },
                    // TODO: move to skinnable container defaults
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Position = new Vector2(-20, 0) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
                new LegacyRankingPanel(),
                new LegacyRankingGraph(),
                new LegacyRankingGrade(),
                new SkinnableModDisplay
                {
                    // TODO: move to skinnable container defaults
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.CentreRight,
                    Position = new Vector2(-20, 260) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Scale = new Vector2(1.5f),
                },
                new LegacyRankingRetryButton
                {
                    // TODO: move to skinnable container defaults
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.CentreRight,
                    Position = new Vector2(0, 360) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
                new LegacyRankingWatchReplayButton()
                {
                    // TODO: move to skinnable container defaults
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.CentreRight,
                    Position = new Vector2(0, 420) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
                new LegacyButton
                {
                    AccentColour = Colour4.BlueViolet,
                    Text = "▼ Online Ranking ▼",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopLeft,
                    Size = new Vector2(200, 30) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Position = new Vector2(-100, -26) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            // TODO: this is a double hack
            // - `SkinnableModDisplay` binds to the global mods bindable to read mods to display
            //   hacking stuff here seems *marginally* less evil than adjusting that component to receive an `IScoreInfo` and magically decide what to show
            // - also `IScoreInfo` hasn't got mods exposed and needs an interface for mods
            Mods.Value = (score.Value as ScoreInfo)?.Mods ?? [];
        }
    }
}
