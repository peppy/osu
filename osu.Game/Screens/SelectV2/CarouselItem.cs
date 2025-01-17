// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// Represents a single display item for display in a <see cref="Carousel{T}"/>.
    /// This is used to house information related to the attached model that helps with display and tracking.
    /// </summary>
    public abstract class CarouselItem : IComparable<CarouselItem>
    {
        /// <summary>
        /// The model this item is representing.
        /// </summary>
        public readonly object Model;

        /// <summary>
        /// The current Y position in the carousel.
        /// This is managed by <see cref="Carousel{T}"/> and should not be set manually.
        /// </summary>
        public double CarouselYPosition { get; set; }

        /// <summary>
        /// The height this item will take when displayed.
        /// </summary>
        public abstract float DrawHeight { get; }

        /// <summary>
        /// Whether this item should be a valid target for user group selection hotkeys.
        /// </summary>
        public bool IsGroupSelectionTarget { get; set; }

        /// <summary>
        /// Whether this item is visible or collapsed (hidden).
        /// </summary>
        public bool IsVisible { get; set; } = true;

        protected CarouselItem(object model)
        {
            Model = model;
        }

        public int CompareTo(CarouselItem? other)
        {
            if (other == null) return 1;

            return CarouselYPosition.CompareTo(other.CarouselYPosition);
        }
    }
}
