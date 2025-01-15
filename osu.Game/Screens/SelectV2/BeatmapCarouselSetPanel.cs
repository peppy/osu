// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Carousel;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCarouselSetPanel : PoolableDrawable, ICarouselPanel
    {
        [Resolved]
        private BeatmapCarousel carousel { get; set; } = null!;

        public CarouselItem? Item
        {
            get => item;
            set
            {
                item = value;

                selected.UnbindBindings();

                if (item != null)
                    selected.BindTo(item.Selected);
            }
        }

        private readonly BindableBool selected = new BindableBool();
        private CarouselItem? item;

        [BackgroundDependencyLoader]
        private void load()
        {
            selected.BindValueChanged(value =>
            {
                if (value.NewValue)
                {
                    Colour = Color4.Pink;
                }
                else
                {
                    Colour = Color4.White;
                }
            });
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();
            Item = null;
        }

        // While we are using the old panels for display purposes, let's inhibit them from handling input.
        // If we don't do this, they will select themselves.
        protected override bool ShouldBeConsideredForInput(Drawable child) => false;

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            DrawYPosition = Item.CarouselYPosition;

            Size = new Vector2(500, Item.DrawHeight);

            InternalChildren = new Drawable[]
            {
                new DrawableCarouselBeatmapSet
                {
                    Item = new CarouselBeatmapSet((BeatmapSetInfo)Item.Model)
                    {
                        State = { Value = CarouselItemState.NotSelected }
                    },
                },
            };

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel.CurrentSelection = Item!.Model;
            return true;
        }

        public double DrawYPosition { get; set; }
    }
}
