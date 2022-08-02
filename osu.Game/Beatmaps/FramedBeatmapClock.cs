// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Beatmaps
{
    public class FramedBeatmapClock : IFrameBasedClock
    {
        private readonly OffsetCorrectionClock finalOffsetClock;

        public FramedBeatmapClock(Track track)
        {
            BindableDouble pauseFreqAdjust = new BindableDouble();

            // the final usable gameplay clock with user-set offsets applied.
            var userGlobalOffsetClock = new OffsetCorrectionClock(track, pauseFreqAdjust);

            // todo: bind with realm
            finalOffsetClock = new OffsetCorrectionClock(userGlobalOffsetClock, pauseFreqAdjust);
        }
    }
}
