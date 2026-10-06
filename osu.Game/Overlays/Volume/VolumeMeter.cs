// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public partial class VolumeMeter : Container, IStateful<SelectionState>
    {
        private CircularProgress volumeCircle = null!;

        public BindableDouble Bindable { get; } = new BindableDouble { MinValue = 0, MaxValue = 1, Precision = 0.01 };

        protected const float CIRCLE_SIZE = 140;

        private readonly Color4 meterColour;
        private readonly LocalisableString name;

        private OsuSpriteText text = null!;

        private Container selectedGlowContainer = null!;

        private Sample? hoverSample;
        private Sample notchSample = null!;

        private double sampleLastPlaybackTime;

        public event Action<SelectionState>? StateChanged;

        private SelectionState state;

        public SelectionState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;
                StateChanged?.Invoke(value);

                updateSelectedState();
            }
        }

        private const float transition_length = 500;

        public VolumeMeter(LocalisableString name, Color4 meterColour)
        {
            this.meterColour = meterColour;
            this.name = name;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            hoverSample = audio.Samples.Get($@"UI/{HoverSampleSet.Button.GetResourceName()}-hover");
            notchSample = audio.Samples.Get(@"UI/notch-tick");
            sampleLastPlaybackTime = Time.Current;

            CircularProgress bgProgress;

            const float progress_start_radius = 0.8f;
            const float progress_size = 0.2f;
            const float progress_end_radius = progress_start_radius + progress_size;

            Children = new Drawable[]
            {
                content = new Container
                {
                    Size = new Vector2(CIRCLE_SIZE),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new CircularContainer
                                {
                                    Masking = true,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Rotation = 225,
                                    Size = new Vector2(progress_end_radius),
                                    Children = new Drawable[]
                                    {
                                        bgProgress = new CircularProgress
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            InnerRadius = 1 - progress_start_radius,
                                            RoundedCaps = true,
                                            Colour = meterColour.Darken(0.5f),
                                        },
                                        volumeCircle = new CircularProgress
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            InnerRadius = 1 - progress_start_radius,
                                            Blending = BlendingParameters.Additive,
                                            Colour = meterColour,
                                            RoundedCaps = true,
                                        },
                                        new Circle
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Scale = new Vector2(progress_start_radius),
                                            Colour = meterColour.Darken(1.5f),
                                            EdgeEffect = new EdgeEffectParameters
                                            {
                                                Radius = 5,
                                                Colour = Color4.Black.Opacity(0.1f),
                                                Type = EdgeEffectType.Shadow,
                                            },
                                        },
                                    }
                                },
                            },
                        },
                        selectedGlowContainer = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(progress_start_radius + 0.01f),
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            BorderColour = meterColour.Lighten(3),
                            BorderThickness = 4,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Glow,
                                Colour = meterColour.Darken(1.8f).Opacity(0.8f),
                                Hollow = true,
                                Radius = 10,
                            }
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Y = -6,
                            Font = OsuFont.Torus.With(size: CIRCLE_SIZE * 0.3f, weight: FontWeight.Light, fixedWidth: true),
                            Blending = BlendingParameters.Additive,
                            Colour = meterColour.Lighten(0.4f),
                            Spacing = new Vector2(-4 * CIRCLE_SIZE / 140, 0),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Y = -32,
                            Font = OsuFont.GetFont(weight: FontWeight.Medium),
                            Blending = BlendingParameters.Additive,
                            Colour = meterColour,
                            Text = name
                        }
                    }
                },
            };

            Bindable.BindValueChanged(volume => { this.TransformTo(nameof(DisplayVolume), volume.NewValue, 400, Easing.OutQuint); }, true);

            bgProgress.Progress = 0.75f;
        }

        private int? displayVolumeInt;

        private double displayVolume;

        protected double DisplayVolume
        {
            get => displayVolume;
            set
            {
                displayVolume = value;

                int intValue = (int)Math.Round(displayVolume * 100);
                bool intVolumeChanged = intValue != displayVolumeInt;

                displayVolumeInt = intValue;

                if (displayVolume >= 0.995f)
                {
                    // TODO: show max
                }

                text.Text = intValue.ToString(CultureInfo.CurrentCulture);

                volumeCircle.Progress = displayVolume * 0.75f;

                if (intVolumeChanged && IsLoaded)
                    Scheduler.AddOnce(playTickSound);
            }
        }

        private void playTickSound()
        {
            const int tick_debounce_time = 30;

            if (Time.Current - sampleLastPlaybackTime <= tick_debounce_time)
                return;

            var channel = notchSample.GetChannel();

            channel.Frequency.Value = 0.99f + RNG.NextDouble(0.02f) + displayVolume * 0.1f;

            // intentionally pitched down, even when hitting max.
            if (displayVolumeInt == 0 || displayVolumeInt == 100)
                channel.Frequency.Value -= 0.5f;

            channel.Play();
            sampleLastPlaybackTime = Time.Current;
        }

        public double Volume
        {
            get => Bindable.Value;
            private set => Bindable.Value = value;
        }

        private const double adjust_step = 0.01;

        public void Increase(double amount = 1, bool isPrecise = false) => adjust(amount, isPrecise);
        public void Decrease(double amount = 1, bool isPrecise = false) => adjust(-amount, isPrecise);

        // because volume precision is set to 0.01, this local is required to keep track of more precise adjustments and only apply when possible.
        private double scrollAccumulation;

        private double accelerationModifier = 1;

        private const double max_acceleration = 5;
        private const double acceleration_multiplier = 1.8;

        private ScheduledDelegate? accelerationDebounce;

        private void resetAcceleration() => accelerationModifier = 1;

        private float dragDelta;

        private Container content = null!;

        protected override bool OnMouseDown(MouseDownEvent e) => true; // handle to prevent drawables behind from potentially receiving the mouse down

        protected override bool OnDragStart(DragStartEvent e)
        {
            dragDelta = 0;
            adjustFromDrag(e.Delta);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            adjustFromDrag(e.Delta);
            base.OnDrag(e);
        }

        private void adjustFromDrag(Vector2 delta)
        {
            const float mouse_drag_divisor = 200;

            dragDelta += delta.Y / mouse_drag_divisor;

            if (Math.Abs(dragDelta) < 0.01) return;

            Volume -= dragDelta;
            dragDelta = 0;
        }

        private void adjust(double delta, bool isPrecise)
        {
            if (delta == 0)
                return;

            // every adjust increment increases the rate at which adjustments happen up to a cutoff.
            // this debounce will reset on inactivity.
            accelerationDebounce?.Cancel();
            accelerationDebounce = Scheduler.AddDelayed(resetAcceleration, 150);

            delta *= accelerationModifier;
            accelerationModifier = Math.Min(max_acceleration, accelerationModifier * acceleration_multiplier);

            double precision = Bindable.Precision;

            if (isPrecise)
            {
                scrollAccumulation += delta * adjust_step;

                while (Precision.AlmostBigger(Math.Abs(scrollAccumulation), precision))
                {
                    Volume += Math.Sign(scrollAccumulation) * precision;
                    scrollAccumulation = scrollAccumulation < 0 ? Math.Min(0, scrollAccumulation + precision) : Math.Max(0, scrollAccumulation - precision);
                }
            }
            else
            {
                Volume += Math.Sign(delta) * Math.Max(precision, Math.Abs(delta * adjust_step));
            }
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            adjust(e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (GetContainingInputManager()!.CurrentState.Mouse.Buttons.HasAnyButtonPressed)
                return false;

            State = SelectionState.Selected;
            return base.OnMouseMove(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (GetContainingInputManager()!.CurrentState.Mouse.Buttons.HasAnyButtonPressed)
                return false;

            State = SelectionState.Selected;
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
        }

        private void updateSelectedState()
        {
            switch (state)
            {
                case SelectionState.Selected:
                    content.ScaleTo(1.08f, 800, Easing.OutPow10);
                    selectedGlowContainer.FadeIn(transition_length, Easing.OutExpo);
                    hoverSample?.Play();
                    break;

                case SelectionState.NotSelected:
                    content.ScaleTo(1f, 800, Easing.OutPow10);
                    selectedGlowContainer.FadeOut(transition_length, Easing.Out);
                    break;
            }
        }
    }
}
