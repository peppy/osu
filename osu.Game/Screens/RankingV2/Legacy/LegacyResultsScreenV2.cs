// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

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
                new LegacyRankingTitle(),
                new LegacyRankingPanel(),
            ];
        }
    }
}
