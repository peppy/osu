// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselItem : CarouselItem
    {
        public readonly Guid ID;

        /// <summary>
        /// Whether this item is a group header.
        /// Group headers are generally larger in display. Setting this will account for the size difference.
        /// </summary>
        public bool IsGroupHeader { get; set; }

        public override float DrawHeight => IsGroupHeader ? 80 : 40;

        public BeatmapCarouselItem(object model)
            : base(model)
        {
            ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();

            // This would be true for beatmap set headers in general..
            // unless we have no set headers, at which point difficulties become group targets.
            IsGroupSelectionTarget = IsGroupHeader;
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
