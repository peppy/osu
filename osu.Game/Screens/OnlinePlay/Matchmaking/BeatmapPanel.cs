// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class BeatmapPanel : CompositeDrawable
    {
        private readonly APIBeatmapSet beatmapSet;

        public BeatmapPanel(APIBeatmap beatmap, APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(300, 70);
            Masking = true;
            CornerRadius = 6;

            InternalChildren = new Drawable[]
            {
                new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.Card)
                {
                    RelativeSizeAxes = Axes.Both,
                    OnlineInfo = beatmapSet
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.3f,
                },
            };
        }
    }
}
