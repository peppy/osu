// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A slider bar which adds a "x" to the end of the tooltip string.
    /// </summary>
    public partial class MultiplierSettingsSlider : FormSliderBar<double>
    {
        public MultiplierSettingsSlider()
        {
            KeyboardStep = 0.01f;
            TooltipFormat = f => $"{f}x";
        }
    }
}
