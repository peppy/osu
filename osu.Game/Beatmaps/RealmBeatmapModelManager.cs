// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using osu.Game.Stores;

namespace osu.Game.Beatmaps
{
    public class RealmBeatmapModelManager : IBeatmapModelManager<RealmBeatmapSet>
    {
        public RealmBeatmapModelManager(Storage storage, RealmContextFactory contextFactory, RealmRulesetStore rulesets, GameHost host)
        {
        }

        #region Implementation of IPostNotifications

        public Action<Notification> PostNotification { get; set; }

        #endregion

        #region Implementation of ICanAcceptFiles

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ILive<RealmBeatmapSet>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            throw new NotImplementedException();
        }

        public Task<ILive<RealmBeatmapSet>> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ILive<RealmBeatmapSet>> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ILive<RealmBeatmapSet>> Import(RealmBeatmapSet item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        Task IModelImporter<RealmBeatmapSet>.Import(params string[] paths)
        {
            throw new NotImplementedException();
        }

        public Task Import(params ImportTask[] tasks)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> HandledExtensions { get; } = Array.Empty<string>();

        #endregion

        #region Implementation of IModelManager<RealmBeatmapSet>

        public IBindable<WeakReference<RealmBeatmapSet>> ItemUpdated { get; } = new Bindable<WeakReference<RealmBeatmapSet>>();
        public IBindable<WeakReference<RealmBeatmapSet>> ItemRemoved { get; } = new Bindable<WeakReference<RealmBeatmapSet>>();

        public Task ImportFromStableAsync(StableStorage stableStorage)
        {
            throw new NotImplementedException();
        }

        public void Export(RealmBeatmapSet item)
        {
            throw new NotImplementedException();
        }

        public void ExportModelTo(RealmBeatmapSet model, Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public void Update(RealmBeatmapSet item)
        {
            throw new NotImplementedException();
        }

        public bool Delete(RealmBeatmapSet item)
        {
            throw new NotImplementedException();
        }

        public void Delete(List<RealmBeatmapSet> items, bool silent = false)
        {
            throw new NotImplementedException();
        }

        public void Undelete(List<RealmBeatmapSet> items, bool silent = false)
        {
            throw new NotImplementedException();
        }

        public void Undelete(RealmBeatmapSet item)
        {
            throw new NotImplementedException();
        }

        public bool IsAvailableLocally(RealmBeatmapSet model)
            => false; // TODO: implement

        #endregion

        #region Implementation of IBeatmapModelManager<RealmBeatmapSet>

        public BeatmapOnlineLookupQueue OnlineLookupQueue { get; set; }
        public IWorkingBeatmapCache WorkingBeatmapCache { get; set; }

        #endregion

        #region Implementation of IPostImports<out RealmBeatmapSet>

        public Action<IEnumerable<ILive<RealmBeatmapSet>>> PostImport { get; set; }

        #endregion
    }
}
