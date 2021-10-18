// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public abstract class BeatmapDownloadTrackingComposite : DownloadTrackingComposite<RealmBeatmapSet, RealmBeatmapManager>
    {
        public Bindable<RealmBeatmapSet> BeatmapSet => Model;

        protected BeatmapDownloadTrackingComposite(RealmBeatmapSet set = null)
            : base(set)
        {
        }
    }
}
