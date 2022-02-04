// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class BeatDivisorControl : CompositeDrawable
    {
        private DivisorPanelText panelText;
        private DivisorText divisorText;
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        public BeatDivisorControl(BindableBeatDivisor beatDivisor)
        {
            this.beatDivisor.BindTo(beatDivisor);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Name = "Gray Background",
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray4
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Name = "Black Background",
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black
                                    },
                                    new TickSliderBar(beatDivisor)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Gray4
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronLeft,
                                                        Action = beatDivisor.PreviousDivisor
                                                    },
                                                    divisorText = new DivisorText(beatDivisor),
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronRight,
                                                        Action = beatDivisor.NextDivisor
                                                    }
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.Absolute, 20),
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 20)
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 5 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new TextFlowContainer(s => s.Font = OsuFont.Default.With(size: 11))
                                                    {
                                                        Padding = new MarginPadding { Horizontal = 0 },
                                                        Text = "beat snap divisor",
                                                        RelativeSizeAxes = Axes.Both,
                                                        TextAnchor = Anchor.TopCentre
                                                    },
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 5 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronLeft,
                                                        Action = beatDivisor.PreviousPanel,
                                                        //Anchor = Anchor.BottomRight
                                                    },
                                                    panelText = new DivisorPanelText(beatDivisor),
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronRight,
                                                        Action = beatDivisor.NextPanel,
                                                        //Anchor = Anchor.BottomRight
                                                    }
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 50),
                                                new Dimension()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 25),
                        new Dimension(GridSizeMode.Absolute, 25),
                        new Dimension(GridSizeMode.Absolute, 11),
                        new Dimension(GridSizeMode.Absolute, 25),
                    }
                }
            };

            beatDivisor.BindValueChanged(updateDivisorsText);
            beatDivisor.ValidDivisorsChanged += updatePanelText;

            updatePanelText();
        }

        private void updateDivisorsText(ValueChangedEvent<int> divisor) => divisorText.Text = $"1/{divisor.NewValue}";
        private void updatePanelText() => panelText.Text = $"{beatDivisor.Panel}";

        private class DivisorPanelText : SpriteText
        {
            private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

            public DivisorPanelText(BindableBeatDivisor bd)
            {
                beatDivisor.BindTo(bd);

                Font = OsuFont.Default.With(size: 12);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
            }
        }

        private class DivisorText : SpriteText
        {
            private readonly Bindable<int> beatDivisor = new Bindable<int>();

            public DivisorText(BindableBeatDivisor bd)
            {
                beatDivisor.BindTo(bd);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
            }
        }

        private class DivisorButton : IconButton
        {
            public DivisorButton()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                // Small offset to look a bit better centered along with the divisor text
                Y = 1;

                Size = new Vector2(20);
                IconScale = new Vector2(0.6f);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconColour = Color4.Black;
                HoverColour = colours.Gray7;
                FlashColour = colours.Gray9;
            }
        }

        private class TickSliderBar : SliderBar<int>
        {
            private Marker marker;

            private readonly BindableBeatDivisor beatDivisor;

            public TickSliderBar(BindableBeatDivisor bd)
            {
                CurrentNumber.BindTo(beatDivisor = bd);

                Padding = new MarginPadding { Horizontal = 5 };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                //foreach (var t in availableDivisors)
                //{
                //    AddInternal(new Tick(t)
                //    {
                //        Anchor = Anchor.TopLeft,
                //        Origin = Anchor.TopCentre,
                //        RelativePositionAxes = Axes.X,
                //        X = getMappedPosition(t)
                //    });
                //}

                //AddInternal(marker = new Marker());

                //CurrentNumber.ValueChanged += v =>
                //{
                //    marker.MoveToX(getMappedPosition(v), 100, Easing.OutQuint);
                //    marker.Flash();
                //};
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                beatDivisor.ValidDivisorsChanged += updateDivisors;
                updateDivisors();
            }

            private void updateDivisors()
            {
                for (int i = Children.Count - 1; i >= 0; i--)
                    RemoveInternal(Children[i]); // Remove all ticks to change the panel

                foreach (int t in beatDivisor.ValidDivisors)
                {
                    AddInternal(new Tick(t)
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        X = getMappedPosition(t)
                    });
                }

                AddInternal(marker = new Marker());
                CurrentNumber.BindValueChanged(moveMarker, true);
            }

            private void moveMarker(ValueChangedEvent<int> pos)
            {
                marker.MoveToX(getMappedPosition(pos.NewValue), 100, Easing.OutQuint);
                marker.Flash();
            }

            protected override void UpdateValue(float value)
            {
            }

            public override bool HandleNonPositionalInput => IsHovered && !CurrentNumber.Disabled;

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        beatDivisor.NextDivisor();
                        return true;

                    case Key.Left:
                        beatDivisor.PreviousDivisor();
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                marker.Active = true;
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                base.OnMouseUp(e);
                marker.Active = false;
            }

            protected override bool OnClick(ClickEvent e)
            {
                handleMouseInput(e.CurrentState);
                return true;
            }

            protected override bool OnDragStart(DragStartEvent e) => true; // todo: check

            protected override void OnDrag(DragEvent e)
            {
                handleMouseInput(e.CurrentState);
            }

            private void handleMouseInput(InputState state)
            {
                // copied from SliderBar so we can do custom spacing logic.
                float xPosition = (ToLocalSpace(state?.Mouse.Position ?? Vector2.Zero).X - RangePadding) / UsableWidth;

                CurrentNumber.Value = beatDivisor.ValidDivisors.OrderBy(d => Math.Abs(getMappedPosition(d) - xPosition)).First();
            }

            private float getMappedPosition(float divisor) => (float)Math.Pow((divisor - 1) / (beatDivisor.ValidDivisors.Last() - 1), 0.90f);

            private class Tick : CompositeDrawable
            {
                private readonly int divisor;

                public Tick(int d)
                {
                    divisor = d;
                    Size = new Vector2(2.5f, 10);

                    InternalChild = new Box { RelativeSizeAxes = Axes.Both };

                    CornerRadius = 0.5f;
                    Masking = true;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                }
            }

            private class Marker : CompositeDrawable
            {
                private Color4 defaultColour;

                private const float size = 7;

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Colour = defaultColour = colours.Gray4;
                    Anchor = Anchor.TopLeft;
                    Origin = Anchor.TopCentre;

                    Width = size;
                    RelativeSizeAxes = Axes.Y;
                    RelativePositionAxes = Axes.X;

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            Width = 2,
                            RelativeSizeAxes = Axes.Y,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0.2f), Color4.White),
                            Blending = BlendingParameters.Additive,
                        },
                        new EquilateralTriangle
                        {
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            Height = size,
                            EdgeSmoothness = new Vector2(1),
                            Colour = Color4.White,
                        }
                    };
                }

                private bool active;

                public bool Active
                {
                    get => active;
                    set
                    {
                        this.FadeColour(value ? Color4.White : defaultColour, 500, Easing.OutQuint);
                        active = value;
                    }
                }

                public void Flash()
                {
                    bool wasActive = active;

                    Active = true;

                    if (wasActive) return;

                    using (BeginDelayedSequence(50))
                        Active = false;
                }
            }
        }
    }
}
