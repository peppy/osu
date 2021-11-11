// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmap : CarouselItem
    {
        public override float TotalHeight => DrawableCarouselBeatmap.HEIGHT;

        public readonly ILive<RealmBeatmap> BeatmapInfo;

        public CarouselBeatmap(ILive<RealmBeatmap> beatmapInfo)
        {
            BeatmapInfo = beatmapInfo;
            State.Value = CarouselItemState.Collapsed;
        }

        public override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            var b = BeatmapInfo.Value;

            bool match =
                criteria.Ruleset == null ||
                b.Ruleset.OnlineID == criteria.Ruleset.ID ||
                (b.Ruleset.OnlineID == 0 && criteria.Ruleset.ID > 0 && criteria.AllowConvertedBeatmaps);

            if (b.BeatmapSet?.Equals(criteria.SelectedBeatmapSet) == true)
            {
                // only check ruleset equality or convertability for selected beatmap
                Filtered.Value = !match;
                return;
            }

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(b.StarRating);
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(b.Difficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(b.Difficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(b.Difficulty.CircleSize);
            match &= !criteria.OverallDifficulty.HasFilter || criteria.OverallDifficulty.IsInRange(b.Difficulty.OverallDifficulty);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(b.Length);
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(b.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(b.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(b.Status);

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(b.Metadata.Author.Username);
            match &= !criteria.Artist.HasFilter || criteria.Artist.Matches(b.Metadata.Artist) ||
                     criteria.Artist.Matches(b.Metadata.ArtistUnicode);

            match &= !criteria.UserStarDifficulty.HasFilter || criteria.UserStarDifficulty.IsInRange(b.StarRating);

            if (match)
            {
                string[] terms = b.GetSearchableTerms();

                foreach (string criteriaTerm in criteria.SearchTerms)
                    match &= terms.Any(term => term.Contains(criteriaTerm, StringComparison.InvariantCultureIgnoreCase));

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && criteria.SearchNumber.HasValue)
                {
                    match = (b.OnlineID == criteria.SearchNumber.Value) ||
                            (b.BeatmapSet?.OnlineID == criteria.SearchNumber.Value);
                }
            }

            // TODO: reimplement.
            // if (match)
            //     match &= criteria.Collection?.Beatmaps.Contains(BeatmapInfo) ?? true;
            //
            // if (match && criteria.RulesetCriteria != null)
            //     match &= criteria.RulesetCriteria.Matches(BeatmapInfo);

            Filtered.Value = !match;
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmap otherBeatmap))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Difficulty:
                    int ruleset = BeatmapInfo.Value.Ruleset.OnlineID.CompareTo(otherBeatmap.BeatmapInfo.Value.Ruleset.OnlineID);
                    if (ruleset != 0) return ruleset;

                    return BeatmapInfo.Value.StarRating.CompareTo(otherBeatmap.BeatmapInfo.Value.StarRating);
            }
        }

        public override string ToString() => BeatmapInfo.ToString();
    }
}
