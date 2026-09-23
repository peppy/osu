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
    public partial class LegacyRankingBackgroundOverlay : CompositeDrawable
    {
        private Sprite sprite = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;

            // TODO: move to skinnable container defaults
            Anchor = Anchor.TopRight;
            Origin = Anchor.Centre;
            bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;
            // crosscheck the minus on the X position here - stable doesn't have it but it doesn't make much sense otherwise
            // also the position spec should live on the skinnable container or whatever
            Position = new Vector2(-180, useNewLayout ? 200 : 170) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            InternalChild = sprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = skin.GetTexture(@"ranking-background-overlay"),
                Blending = BlendingParameters.Additive,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sprite.RotateTo(0).Then().RotateTo(360, 20000).Loop();
        }
    }
}
