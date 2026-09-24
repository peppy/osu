// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
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
                new LegacyRankingDetails(),
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
