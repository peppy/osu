// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    internal partial class SpectatorScoreBar : CompositeDrawable
    {
        private FillFlowContainer flow = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = SpectatorSongBar.HEIGHT * 1.5f;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Blue4,
                    RelativeSizeAxes = Axes.Both,
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                }
            };
        }
    }
}
