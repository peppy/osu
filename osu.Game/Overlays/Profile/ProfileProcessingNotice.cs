// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Profile
{
    public partial class ProfileProcessingNotice : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OverlayColourProvider colourProvider, OsuColour colours)
        {
            if (string.IsNullOrEmpty(api.ScoreProcessingNoticeUrl))
                return;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            LinkFlowContainer flow;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Vertical = 10, Horizontal = 50 },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            CornerRadius = 5,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background4,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                flow = new LinkFlowContainer(cp =>
                                {
                                    cp.Colour = colours.Orange1;
                                    cp.Font = OsuFont.Style.Body;
                                })
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(10),
                                }
                            }
                        },
                    }
                },
            };

            flow.AddIcon(FontAwesome.Solid.InfoCircle);
            flow.AddText(" wangs");
        }
    }
}
