// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapCarouselV2 : CompositeDrawable
    {
        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            detachedBeatmaps = beatmapStore.GetBeatmaps(cancellationToken);
            detachedBeatmaps.BindCollectionChanged(beatmapsChanged, true);
        }

        private void beatmapsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }
    }
}
