// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class BeatmapSelectionPanel : Container
    {
        public Action? Clicked;

        protected override Container<Drawable> Content { get; }

        private readonly Container scaleContainer;
        private readonly Container<SelectionAvatar> avatarContainer;
        private readonly Box flash;

        private readonly Dictionary<int, SelectionAvatar> pills = new Dictionary<int, SelectionAvatar>();

        public BeatmapSelectionPanel(float width, float height)
        {
            Size = new Vector2(width, height);
            InternalChild = scaleContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        // TODO: corner radius should absolutely not be done here
                        CornerRadius = 6,
                        Child = flash = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        },
                    },
                    avatarContainer = new Container<SelectionAvatar>
                    {
                        RelativeSizeAxes = Axes.X,
                        Padding = new MarginPadding { Horizontal = 10 },
                        AutoSizeAxes = Axes.Y,
                        Origin = Anchor.CentreLeft,
                    },
                    new HoverClickSounds()
                }
            };
        }

        public bool AddUser(APIUser user, bool self = false)
        {
            if (pills.ContainsKey(user.Id))
                return false;

            var avatar = new SelectionAvatar(user, self)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            };

            avatarContainer.Add(pills[user.Id] = avatar);

            updateLayout();

            avatar.FinishTransforms();

            return true;
        }

        public bool RemoveUser(APIUser user)
        {
            if (!pills.Remove(user.Id, out var pill))
                return false;

            pill.PopOutAndExpire();
            avatarContainer.ChangeChildDepth(pill, float.MaxValue);

            updateLayout();

            return true;
        }

        private void updateLayout()
        {
            const double stagger = 30;
            const float spacing = 4;

            double delay = 0;
            float x = 0;

            for (int i = avatarContainer.Count - 1; i >= 0; i--)
            {
                var avatar = avatarContainer[i];

                if (avatar.Expired)
                    continue;

                avatar.Delay(delay).MoveToX(x, 500, Easing.OutElasticQuarter);

                x -= avatar.LayoutSize.X + spacing;

                delay += stagger;
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                scaleContainer.ScaleTo(0.9f, 400, Easing.OutExpo);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Left)
                scaleContainer.ScaleTo(1f, 500, Easing.OutElasticHalf);
        }

        protected override bool OnHover(HoverEvent e)
        {
            flash.FadeTo(0.3f, 50)
                 .Then()
                 .FadeTo(0.15f, 300);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            flash.FadeOut(200);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Clicked?.Invoke();

            flash.FadeTo(0.6f, 50)
                 .Then()
                 .FadeTo(0.15f, 400);

            return true;
        }

        public partial class SelectionAvatar : CompositeDrawable
        {
            public bool Expired { get; private set; }

            private readonly Container content;

            private readonly bool self;

            public SelectionAvatar(APIUser user, bool self)
            {
                this.self = self;

                Size = new Vector2(30);

                InternalChildren = new Drawable[]
                {
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.LightSlateGray,
                                },
                                new ClickableAvatar(user, true)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                if (self)
                {
                    content.Add(new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = 1,
                        Padding = new MarginPadding(-2),
                        Child = new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colour.Yellow,
                        }
                    });
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                content.ScaleTo(0)
                       .ScaleTo(1, 500, Easing.OutElasticHalf)
                       .FadeIn(200);
            }

            public void PopOutAndExpire()
            {
                content.ScaleTo(0, 300, Easing.OutExpo);

                this.Delay(300).FadeOut().Expire();
                Expired = true;
            }
        }
    }
}
