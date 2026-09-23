// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    public partial class LegacyRankingTitle : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;

            // TODO: move to skinnable container defaults
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            Position = new Vector2(-20, 0) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            InternalChild = new Sprite
            {
                Texture = skin.GetTexture(@"ranking-title"),
            };
        }
    }
}
