// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public interface IConfigurableDrawable : ISerialisableDrawable
    {
        /// <summary>
        /// Apply any default layout which would subsequently be overridden by user modifications.
        /// Will be called to restore a sane and known state.
        /// </summary>
        void ApplyDefaults();
    }
}
