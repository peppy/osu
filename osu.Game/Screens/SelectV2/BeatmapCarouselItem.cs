// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselItem : CarouselItem
    {
        public readonly Guid ID;

        /// <summary>
        /// Whether this item has a header providing extra information for it.
        /// When displaying items which don't have header, we should make sure enough information is included inline.
        /// </summary>
        public bool HasGroupHeader { get; set; }

        /// <summary>
        /// Whether this item is a group header.
        /// Group headers are generally larger in display. Setting this will account for the size difference.
        /// </summary>
        public bool IsGroupHeader { get; set; }

        public override float DrawHeight => IsGroupHeader ? DrawableCarouselBeatmapSet.HEIGHT : DrawableCarouselBeatmap.HEIGHT;

        public BeatmapCarouselItem(object model)
            : base(model)
        {
            ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();
        }

        public override string? ToString()
        {
            switch (Model)
            {
                case BeatmapInfo bi:
                    return $"Difficulty: {bi.DifficultyName} ({bi.StarRating:N1}*)";

                case BeatmapSetInfo si:
                    return $"{si.Metadata}";
            }

            return Model.ToString();
        }
    }
}
