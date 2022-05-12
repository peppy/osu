// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class BackButton : VisibilityContainer
    {
        public Action Action;

        private readonly ShearedButton button;

        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Pink);

        public BackButton(Receptor receptor = null)
        {
            Size = new Vector2(200, 50);

            Margin = new MarginPadding { Bottom = 14 };

            Child = button = new ShearedButton(200)
            {
                Text = CommonStrings.Back,
                Action = () => Action?.Invoke(),
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            };

            if (receptor == null)
            {
                // if a receptor wasn't provided, create our own locally.
                Add(receptor = new Receptor());
            }

            receptor.OnBackPressed = () => button.TriggerClick();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            button.DarkerColour = colours.Pink2;
            button.LighterColour = colours.Pink1;
        }

        protected override void PopIn()
        {
            button.MoveToY(0, 400, Easing.OutQuint);
            button.FadeIn(150, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            button.MoveToY(100, 400, Easing.OutQuint);
            button.FadeOut(400, Easing.OutQuint);
        }

        public class Receptor : Drawable, IKeyBindingHandler<GlobalAction>
        {
            public Action OnBackPressed;

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Repeat)
                    return false;

                switch (e.Action)
                {
                    case GlobalAction.Back:
                        OnBackPressed?.Invoke();
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }
    }
}
