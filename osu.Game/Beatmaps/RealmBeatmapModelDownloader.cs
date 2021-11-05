// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps
{
    public class RealmBeatmapModelDownloader : ModelDownloader<RealmBeatmapSet>
    {
        public RealmBeatmapModelDownloader(IModelImporter<RealmBeatmapSet> beatmapModelManager, IAPIProvider api, GameHost host = null)
            : base(beatmapModelManager, api, host)
        {
        }

        protected override ArchiveDownloadRequest<RealmBeatmapSet> CreateDownloadRequest(RealmBeatmapSet set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest<RealmBeatmapSet>(set, minimiseDownloadSize);

        public override ArchiveDownloadRequest<RealmBeatmapSet> GetExistingDownload(RealmBeatmapSet model)
            => CurrentDownloads.FirstOrDefault(req => req.Model.OnlineID == model?.OnlineID);
    }
}
