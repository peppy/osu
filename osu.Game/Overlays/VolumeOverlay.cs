// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays.Volume;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    [Cached]
    public partial class VolumeOverlay : VisibilityContainer
    {
        public Bindable<bool> IsMuted { get; } = new Bindable<bool>();

        private VolumeMeter volumeMeterMaster = null!;
        private VolumeMeter volumeMeterEffect = null!;
        private VolumeMeter volumeMeterMusic = null!;
        private VolumeMeter volumeMeterHitsound = null!;

        // ReSharper disable once NotAccessedField.Local
        private Bindable<double> volumeGameplay = null!;

        private Container<VolumeMeter> volumeMeters = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            volumeMeters.ReceivePositionalInputAt(screenSpacePos);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours, OsuConfigManager config)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            AddRange(new Drawable[]
            {
                // new Box
                // {
                //     RelativeSizeAxes = Axes.X,
                //     Height = 360,
                //     Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.75f))
                // },
                volumeMeters = new Container<VolumeMeter>
                {
                    Size = new Vector2(300),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Children = new[]
                    {
                        volumeMeterMaster = new MasterVolumeMeter(AudioSettingsStrings.MasterVolume.ToUpper(), colours.PinkDarker)
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.BottomLeft,
                            IsMuted = { BindTarget = IsMuted },
                        },
                        volumeMeterMusic = new VolumeMeter(AudioSettingsStrings.MusicVolume.ToUpper(), colours.BlueDarker)
                        {

                            Rotation = angleFor(0),
                            Anchor = Anchor.BottomLeft,
                        },
                        volumeMeterEffect = new VolumeMeter(AudioSettingsStrings.EffectVolume.ToUpper(), colours.BlueDarker)
                        {
                            Rotation = angleFor(1),
                            Anchor = Anchor.BottomLeft,
                        },
                        volumeMeterHitsound = new VolumeMeter(AudioSettingsStrings.HitsoundVolume.ToUpper(), colours.BlueDarker)
                        {
                            Rotation = angleFor(2),
                            Anchor = Anchor.BottomLeft,
                        },
                    }
                },
            });

            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);
            volumeMeterHitsound.Bindable.BindTo(volumeGameplay = config.GetBindable<double>(OsuSetting.HitsoundVolume));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var volumeMeter in volumeMeters)
                volumeMeter.Bindable.ValueChanged += _ => Show();
        }

        public bool Adjust(GlobalAction action, float amount = 1, bool isPrecise = false)
        {
            if (!IsLoaded) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    // else
                    //     volumeMeters.Selected?.Decrease(amount, isPrecise);
                    return true;

                case GlobalAction.IncreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    // else
                    //     volumeMeters.Selected?.Increase(amount, isPrecise);
                    return true;

                case GlobalAction.NextVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    // volumeMeters.SelectNext();
                    Show();
                    return true;

                case GlobalAction.PreviousVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    // volumeMeters.SelectPrevious();
                    Show();
                    return true;

                case GlobalAction.ToggleMute:
                    Show();
                    volumeMeters.OfType<MasterVolumeMeter>().First().ToggleMute();
                    return true;
            }

            return false;
        }

        public void FocusMasterVolume()
        {
            // volumeMeters.Select(volumeMeterMaster);
        }

        public override void Show()
        {
            // Focus on the master meter as a default if previously hidden
            if (State.Value == Visibility.Hidden)
                FocusMasterVolume();

            if (State.Value == Visibility.Visible)
                schedulePopOut();

            base.Show();
        }

        // -100 -> angled upwards titled slightly left
        // 10 -> angled right titled slightly downwards
        private float angleFor(int index) => float.Lerp(-100, 10, index / (float)(3 - 1));

        protected override void PopIn()
        {
            ClearTransforms();
            schedulePopOut();

            volumeMeterMaster.ScaleTo(1f, 600, Easing.OutElasticQuarter)
                             .FadeIn(300);

            for (int i = 1; i < 4; i++)
            {
                volumeMeters[i]
                    .Delay(i * 50)
                    .RotateTo(angleFor(i), 600, Easing.OutElasticQuarter)
                    .ScaleTo(1f, 600, Easing.OutElasticQuarter)
                    .FadeIn(300);
            }

            this.TransformTo(nameof(Padding), new MarginPadding(120), 600, Easing.OutElasticQuarter);
        }

        protected override void PopOut()
        {
            this.FadeOut(300, Easing.OutQuint);

            volumeMeterMaster.ScaleTo(0.5f, 300, Easing.In)
                             .FadeOut(200);

            for (int i = 1; i < 4; i++)
            {
                volumeMeters[i]
                    .Delay((4 - 1 - i) * 30)
                    .RotateTo(angleFor(i) + 90, 200, Easing.In)
                    .ScaleTo(0.5f, 150, Easing.In)
                    .FadeOut(150);
            }

            this.TransformTo(nameof(Padding), new MarginPadding(70), 300, Easing.In);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // keep the scheduled event correctly timed as long as we have movement.
            schedulePopOut();
            return base.OnMouseMove(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            schedulePopOut();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            schedulePopOut();
            base.OnHoverLost(e);
        }

        private ScheduledDelegate? popOutDelegate;

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();

            this.FadeIn(200, Easing.Out);

            if (!IsHovered)
            {
                this.Delay(200)
                    .FadeOut(2000, Easing.In);
            }

            this.Delay(1200).Schedule(() =>
            {
                if (!IsHovered)
                    Hide();
            }, out popOutDelegate);
        }
    }
}
