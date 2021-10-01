// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A beatmap generally made available by a <see cref="Player"/> instance, exposing the final state (post-conversion, post-mod-application) beatmap that is being used for gameplay.
    /// </summary>
    public class GameplayBeatmap : IBeatmap
    {
        private readonly IBeatmap playableBeatmap;

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        public GameplayBeatmap(IBeatmap playableBeatmap)
        {
            this.playableBeatmap = playableBeatmap;
        }

        /// <summary>
        /// A bindable containing the last judgement result applied to any hit object.
        /// </summary>
        public IBindable<JudgementResult> LastJudgementResult => lastJudgementResult;

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="GameplayBeatmap"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        public void ApplyResult(JudgementResult result) => lastJudgementResult.Value = result;

        #region Implementation of IBeatmap (delegated to playableBeatmap)

        public BeatmapInfo BeatmapInfo
        {
            get => playableBeatmap.BeatmapInfo;
            set => playableBeatmap.BeatmapInfo = value;
        }

        public ControlPointInfo ControlPointInfo
        {
            get => playableBeatmap.ControlPointInfo;
            set => playableBeatmap.ControlPointInfo = value;
        }

        public BeatmapMetadata Metadata => playableBeatmap.Metadata;
        public List<BreakPeriod> Breaks => playableBeatmap.Breaks;
        public double TotalBreakTime => playableBeatmap.TotalBreakTime;
        public IReadOnlyList<HitObject> HitObjects => playableBeatmap.HitObjects;
        public IEnumerable<BeatmapStatistic> GetStatistics() => playableBeatmap.GetStatistics();
        public double GetMostCommonBeatLength() => playableBeatmap.GetMostCommonBeatLength();
        public IBeatmap Clone() => playableBeatmap.Clone();

        #endregion
    }
}
