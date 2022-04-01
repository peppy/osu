// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Skinning.Editor
{
    internal class ToolboxButton : OsuButton
    {
        protected const float DEFAULT_SIZE = 60;

        public ToolboxButton()
        {
            RelativeSizeAxes = Axes.X;
            Height = DEFAULT_SIZE;

            Enabled.Value = true;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BackgroundColour = colourProvider.Background3;
        }
    }
}
