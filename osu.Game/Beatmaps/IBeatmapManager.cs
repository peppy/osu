// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapManager : IModelDownloader<BeatmapSetInfo>, IModelManager<BeatmapSetInfo>, IModelFileManager<BeatmapSetInfo, BeatmapSetFileInfo>, IWorkingBeatmapCache, IDisposable
    {
    }
}
