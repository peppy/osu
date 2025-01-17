// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCarouselPanel : PoolableDrawable, ICarouselPanel
    {
        [Resolved]
        private BeatmapCarousel carousel { get; set; } = null!;

        private Box activationFlash = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Selected.BindValueChanged(value =>
            {
                activationFlash.FadeTo(value.NewValue ? 0.2f : 0, 500, Easing.OutQuint);
            });

            KeyboardSelected.BindValueChanged(value =>
            {
                if (value.NewValue)
                {
                    BorderThickness = 5;
                    BorderColour = Color4.Pink;
                }
                else
                {
                    BorderThickness = 0;
                }
            });
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();
            Item = null;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            DrawYPosition = Item.CarouselYPosition;

            Size = new Vector2(500, Item.DrawHeight);
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = (Item.Model is BeatmapInfo ? Color4.Aqua : Color4.Yellow).Darken(5),
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both,
                },
                activationFlash = new Box
                {
                    Colour = Color4.White,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuSpriteText
                {
                    Text = Item.ToString() ?? string.Empty,
                    Padding = new MarginPadding(5),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (carousel.CurrentSelection == Item!.Model)
                carousel.ActivateSelection();
            else
                carousel.CurrentSelection = Item!.Model;
            return true;
        }

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void FlashFromActivation()
        {
            activationFlash.FadeOutFromOne(500, Easing.OutQuint);
        }
    }
}
