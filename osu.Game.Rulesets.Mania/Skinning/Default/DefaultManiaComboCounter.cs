// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Graphics.Colour;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public class DefaultManiaComboCounter : DefaultComboCounter
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();
            Colour = ColourInfo.GradientHorizontal(Color4.Green, Color4.Yellow);
        }
    }
}
