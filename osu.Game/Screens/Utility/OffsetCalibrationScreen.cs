// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility
{
    public partial class OffsetCalibrationScreen : OsuScreen
    {
        public override bool HideOverlaysOnEnter => true;

        public override float BackgroundParallaxAmount => 0;

        [Cached]
        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Orange);

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private readonly OsuTextFlowContainer statusText;

        private Bindable<double> audioOffset = null!;

        public OffsetCalibrationScreen()
        {
            LinkFlowContainer explanatoryText;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColourProvider.Background6,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(20),
                    Children = new Drawable[]
                    {
                        new DisplayContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(300),
                        },
                        explanatoryText = new LinkFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                        },
                        statusText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 40))
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            Y = 150,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        new TimingAdjustButton(1)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Action = adjust => audioOffset.Value += adjust,
                            Text = "Adjust offset",
                            Size = new Vector2(300, 80),
                        },
                    }
                },
            };

            explanatoryText.AddParagraph(@"Welcome to the offset calibrator!");
            explanatoryText.AddParagraph("Adjust the offset until the ticking sound gets quiet (or disappears)");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            audioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            audioOffset.BindValueChanged(offsetChanged, true);
        }

        private void offsetChanged(ValueChangedEvent<double> offset)
        {
            statusText.Text = $"Audio Offset: {LocalisableString.Interpolate($@"{offset.NewValue:0.0} ms")}";
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    audioOffset.Value -= 1;
                    return true;

                case Key.Right:
                    audioOffset.Value += 1;
                    return true;
            }

            return base.OnKeyDown(e);
        }
    }

    public partial class DisplayContainer : BeatSyncedContainer
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Sample? sampleTick;

        private Circle circle = null!;

        public DisplayContainer()
        {
            AllowMistimedEventFiring = false;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleTick = audio.Samples.Get(@"UI/metronome-tick");

            Add(circle = new Circle
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = colours.YellowLight,
            });
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            double length = timingPoint.BeatLength;

            circle
                .ScaleTo(1)
                .ScaleTo(2, length, Easing.OutQuint)
                .FadeOutFromOne(length, Easing.OutQuint);

            sampleTick?.Play();
        }
    }
}
