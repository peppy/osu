// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmap : CarouselItem
    {
        public override float TotalHeight => DrawableCarouselBeatmap.HEIGHT;

        public readonly ILive<BeatmapInfo> BeatmapInfo;

        public CarouselBeatmap(BeatmapInfo beatmapInfo, [CanBeNull] RealmContextFactory realmContextFactory = null)
        {
            BeatmapInfo = realmContextFactory != null
                ? beatmapInfo.ToLive(realmContextFactory)
                : beatmapInfo.ToLiveUnmanaged();

            State.Value = CarouselItemState.Collapsed;
        }

        public override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            var beatmapInfo = BeatmapInfo.Value;

            bool match =
                criteria.Ruleset == null ||
                beatmapInfo.RulesetID == criteria.Ruleset.OnlineID ||
                (beatmapInfo.RulesetID == 0 && criteria.Ruleset.OnlineID > 0 && criteria.AllowConvertedBeatmaps);

            if (beatmapInfo.BeatmapSet?.Equals(criteria.SelectedBeatmapSet) == true)
            {
                // only check ruleset equality or convertability for selected beatmap
                Filtered.Value = !match;
                return;
            }

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(beatmapInfo.StarRating);
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(beatmapInfo.BaseDifficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(beatmapInfo.BaseDifficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(beatmapInfo.BaseDifficulty.CircleSize);
            match &= !criteria.OverallDifficulty.HasFilter || criteria.OverallDifficulty.IsInRange(beatmapInfo.BaseDifficulty.OverallDifficulty);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(beatmapInfo.Length);
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(beatmapInfo.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(beatmapInfo.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(beatmapInfo.Status);

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(beatmapInfo.Metadata.Author.Username);
            match &= !criteria.Artist.HasFilter || criteria.Artist.Matches(beatmapInfo.Metadata.Artist) ||
                     criteria.Artist.Matches(beatmapInfo.Metadata.ArtistUnicode);

            match &= !criteria.UserStarDifficulty.HasFilter || criteria.UserStarDifficulty.IsInRange(beatmapInfo.StarRating);

            if (match)
            {
                string[] terms = beatmapInfo.GetSearchableTerms();

                foreach (string criteriaTerm in criteria.SearchTerms)
                    match &= terms.Any(term => term.Contains(criteriaTerm, StringComparison.InvariantCultureIgnoreCase));

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && criteria.SearchNumber.HasValue)
                {
                    match = (beatmapInfo.OnlineID == criteria.SearchNumber.Value) ||
                            (beatmapInfo.BeatmapSet?.OnlineID == criteria.SearchNumber.Value);
                }
            }

            if (match)
                match &= criteria.Collection?.Beatmaps.Contains(beatmapInfo) ?? true;

            if (match && criteria.RulesetCriteria != null)
                match &= criteria.RulesetCriteria.Matches(beatmapInfo);

            Filtered.Value = !match;
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmap otherBeatmap))
                return base.CompareTo(criteria, other);

            var beatmapInfo = BeatmapInfo.Value;
            var otherBeatmapInfo = otherBeatmap.BeatmapInfo.Value;

            switch (criteria.Sort)
            {
                default:
                case SortMode.Difficulty:
                    int ruleset = beatmapInfo.RulesetID.CompareTo(otherBeatmapInfo.RulesetID);
                    if (ruleset != 0) return ruleset;

                    return beatmapInfo.StarRating.CompareTo(otherBeatmapInfo.StarRating);
            }
        }

        public override string ToString() => BeatmapInfo.ToString();
    }
}
