// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Screens.RankingV2.Legacy
{
    // TODO: action, disabled state, yadda yadda
    public partial class LegacyButton : CompositeDrawable
    {
        private NineSliceSprite backgroundSprite = null!;

        public Colour4 AccentColour { get; init; }

        public LocalisableString Text { get; init; }

        public Action? Action { get; set; }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            InternalChildren =
            [
                backgroundSprite = new NineSliceSprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = skin.GetTexture(@"button"),
                    TextureInset = new MarginPadding { Horizontal = 16, },
                },
                new OsuSpriteText
                {
                    Text = Text,
                    Font = OsuFont.Default.With(size: 14 * Height / 18),
                    UseFullGlyphHeight = false,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            backgroundSprite.FlashColour(Colour4.White, 400);
            return true;
        }

        private void updateState()
        {
            var targetColour = AccentColour;
            const float unhovered_reduction = 20 / 255f;

            if (!IsHovered)
            {
                targetColour = new Colour4(
                    MathF.Max(0, targetColour.R - unhovered_reduction),
                    MathF.Max(0, targetColour.G - unhovered_reduction),
                    MathF.Max(0, targetColour.B - unhovered_reduction),
                    targetColour.A);
            }

            backgroundSprite.Colour = targetColour;
        }
    }
}
