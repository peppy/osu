// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Screens.Play;

namespace osu.Game.Beatmaps
{
    public class FramedBeatmapClock : Component, IFrameBasedClock, IAdjustableClock, ISourceChangeableClock
    {
        private readonly OffsetCorrectionClock finalOffsetClock;

        private Bindable<double> userAudioOffset = null!;

        private IDisposable? beatmapOffsetSubscription;

        private readonly OffsetCorrectionClock userGlobalOffsetClock;

        private readonly DecoupleableInterpolatingFramedClock decoupledClock;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public FramedBeatmapClock(IClock? sourceClock = null)
        {
            // TODO: Unused for now?
            var pauseFreqAdjust = new BindableDouble(1);

            // A decoupled clock is used to ensure precise time values even when the host audio subsystem is not reporting
            // high precision times (on windows there's generally only 5-10ms reporting intervals, as an example).
            decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            decoupledClock.ChangeSource(sourceClock);

            // Audio timings in general with newer BASS versions don't match stable.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            var platformOffsetClock = new OffsetCorrectionClock(decoupledClock, pauseFreqAdjust) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // User global offset (set in settings) should also be applied.
            userGlobalOffsetClock = new OffsetCorrectionClock(platformOffsetClock, pauseFreqAdjust);

            // User per-beatmap offset will be applied to this final clock.
            finalOffsetClock = new OffsetCorrectionClock(userGlobalOffsetClock, pauseFreqAdjust);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => userGlobalOffsetClock.Offset = offset.NewValue, true);

            beatmapOffsetSubscription = realm.SubscribeToPropertyChanged(
                r => r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings,
                settings => settings.Offset,
                val => finalOffsetClock.Offset = val);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
        }

        #region Delegation of IAdjustableClock / ISourceChangeableClock to decoupled clock.

        public void ChangeSource(IClock? source) => decoupledClock.ChangeSource(source);

        public IClock? Source => decoupledClock.Source;

        public void Reset() => decoupledClock.Reset();

        public void Start() => decoupledClock.Start();

        public void Stop() => decoupledClock.Stop();

        public bool Seek(double position) => decoupledClock.Seek(position);

        public void ResetSpeedAdjustments() => decoupledClock.ResetSpeedAdjustments();

        public double Rate
        {
            get => decoupledClock.Rate;
            set => decoupledClock.Rate = value;
        }

        public bool IsCoupled
        {
            get => decoupledClock.IsCoupled;
            set => decoupledClock.IsCoupled = value;
        }

        #endregion

        #region Delegation of IFrameBasedClock to clock with all offsets applied

        public double CurrentTime => finalOffsetClock.CurrentTime;

        public bool IsRunning => finalOffsetClock.IsRunning;

        public void ProcessFrame() => finalOffsetClock.ProcessFrame();

        public double ElapsedFrameTime => finalOffsetClock.ElapsedFrameTime;

        public double FramesPerSecond => finalOffsetClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => finalOffsetClock.TimeInfo;

        #endregion
    }
}
