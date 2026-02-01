// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A <see cref="SettingsSlider{TValue,TSlider}"/> that displays its value as a percentage by default.
    /// Mostly provided for convenience of use with <see cref="SettingSourceAttribute"/>.
    /// </summary>
    public partial class SettingsPercentageSlider<TValue> : FormSliderBar<TValue>
        where TValue : struct, INumber<TValue>, IMinMaxValue<TValue>
    {
        public SettingsPercentageSlider()
        {
            DisplayAsPercentage = true;
        }
    }
}
