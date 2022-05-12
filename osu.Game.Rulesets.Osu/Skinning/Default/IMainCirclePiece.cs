// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public interface IMainCirclePiece
    {
        /// <summary>
        /// A target drawable that can be flashed externally.
        /// </summary>
        Drawable FlashTarget { get; }
    }
}
