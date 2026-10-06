// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
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
        private VolumeMeter volumeMeterGameplay = null!;

        // ReSharper disable once NotAccessedField.Local
        private Bindable<double> volumeGameplay = null!;

        private SelectionCycleFillFlowContainer<VolumeMeter> volumeMeters = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            volumeMeters.ReceivePositionalInputAt(screenSpacePos);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours, OsuConfigManager config)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 360,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.75f))
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -10,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        volumeMeters = new SelectionCycleFillFlowContainer<VolumeMeter>
                        {
                            Direction = FillDirection.Full,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Spacing = new Vector2(10, 0),
                            Children = new[]
                            {
                                volumeMeterMaster = new MasterVolumeMeter(AudioSettingsStrings.MasterVolume, colours.PurpleDark)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    IsMuted = { BindTarget = IsMuted },
                                },
                                volumeMeterEffect = new VolumeMeter(AudioSettingsStrings.EffectVolume, colours.DarkOrange4)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Margin = new MarginPadding { Bottom = 40, Left = -50},
                                    Scale = new Vector2(0.7f),
                                },
                                volumeMeterMusic = new VolumeMeter(AudioSettingsStrings.MusicVolume, colours.Pink4)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Margin = new MarginPadding { Bottom = 40 },
                                    Scale = new Vector2(0.7f),
                                },
                                volumeMeterGameplay = new VolumeMeter(AudioSettingsStrings.GameplayVolume, colours.Lime4)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Margin = new MarginPadding { Bottom = 40 },
                                    Scale = new Vector2(0.7f),
                                },
                            }
                        },
                    },
                },
            });

            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);
            volumeMeterGameplay.Bindable.BindTo(volumeGameplay = config.GetBindable<double>(OsuSetting.GameplayVolume));
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
                    else
                        volumeMeters.Selected?.Decrease(amount, isPrecise);
                    return true;

                case GlobalAction.IncreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else
                        volumeMeters.Selected?.Increase(amount, isPrecise);
                    return true;

                case GlobalAction.NextVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    volumeMeters.SelectNext();
                    Show();
                    return true;

                case GlobalAction.PreviousVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    volumeMeters.SelectPrevious();
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
            volumeMeters.Select(volumeMeterMaster);
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

        protected override void PopIn()
        {
            ClearTransforms();
            schedulePopOut();
        }

        protected override void PopOut()
        {
            this.FadeOut(300, Easing.OutQuint);
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
