// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;
using osu.Game.Stores;

#nullable enable

namespace osu.Game.Beatmaps
{
    public class RealmBeatmapManager : IBeatmapManager<RealmBeatmapSet>
    {
        [UsedImplicitly]
        private readonly RealmContextFactory realmFactory;

        private readonly IModelImporter<RealmBeatmapSet> beatmapImporter;

        private readonly IModelDownloader<RealmBeatmapSet> beatmapModelDownloader;

        private readonly WorkingBeatmapCache workingBeatmapCache;
        private readonly BeatmapOnlineLookupQueue? onlineBeatmapLookupQueue;
        private readonly RealmBeatmapModelManager beatmapModelManager;

        public RealmBeatmapManager(Storage storage, RealmContextFactory realmFactory, RealmRulesetStore rulesets, IAPIProvider api, AudioManager audioManager, IResourceStore<byte[]> resources, GameHost? host = null, WorkingBeatmap? defaultBeatmap = null, bool performOnlineLookups = false)
        {
            if (performOnlineLookups)
                onlineBeatmapLookupQueue = new BeatmapOnlineLookupQueue(api, storage);

            this.realmFactory = realmFactory;

            beatmapImporter = new BeatmapImporter(realmFactory, storage, onlineBeatmapLookupQueue);
            beatmapModelManager = CreateBeatmapModelManager(storage, realmFactory, rulesets, api, host);
            beatmapModelDownloader = CreateBeatmapModelDownloader(beatmapImporter, api, host);
            workingBeatmapCache = CreateWorkingBeatmapCache(audioManager, resources, new RealmFileStore(realmFactory, storage).Store, defaultBeatmap, host);

            // workingBeatmapCache.BeatmapManager = beatmapModelManager;
            beatmapModelManager.WorkingBeatmapCache = workingBeatmapCache;

            if (performOnlineLookups)
            {
                beatmapModelManager.OnlineLookupQueue = onlineBeatmapLookupQueue;
            }
        }

        protected virtual RealmBeatmapModelManager CreateBeatmapModelManager(Storage storage, RealmContextFactory contextFactory, RealmRulesetStore rulesets, IAPIProvider api, GameHost? host) =>
            new RealmBeatmapModelManager(storage, contextFactory, rulesets, host);

        protected virtual RealmBeatmapModelDownloader CreateBeatmapModelDownloader(IModelImporter<RealmBeatmapSet> importer, IAPIProvider api, GameHost? host) =>
            new RealmBeatmapModelDownloader(importer, api, host);

        protected virtual WorkingBeatmapCache CreateWorkingBeatmapCache(AudioManager audioManager, IResourceStore<byte[]> resources, IResourceStore<byte[]> storage, WorkingBeatmap? defaultBeatmap, GameHost? host) =>
            new WorkingBeatmapCache(audioManager, resources, storage, defaultBeatmap, host);

        #region Implementation of IPostNotifications

        public Action<Notification> PostNotification
        {
            set => beatmapImporter.PostNotification = value;
        }

        #endregion

        #region Implementation of ICanAcceptFiles

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            return beatmapImporter.Import(paths);
        }

        public Task<IEnumerable<ILive<RealmBeatmapSet>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            return beatmapImporter.Import(notification, tasks);
        }

        public Task<ILive<RealmBeatmapSet>> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapImporter.Import(task, lowPriority, cancellationToken);
        }

        public Task<ILive<RealmBeatmapSet>> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapImporter.Import(archive, lowPriority, cancellationToken);
        }

        public Task<ILive<RealmBeatmapSet>> Import(RealmBeatmapSet item, ArchiveReader? archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapImporter.Import(item, archive, lowPriority, cancellationToken);
        }

        Task IModelImporter<RealmBeatmapSet>.Import(params string[] paths)
        {
            return beatmapImporter.Import(paths);
        }

        public Task Import(params ImportTask[] tasks)
        {
            return beatmapImporter.Import(tasks);
        }

        public IEnumerable<string> HandledExtensions => beatmapImporter.HandledExtensions;

        #endregion

        #region Implementation of IModelDownloader<RealmBeatmapSet>

        public IBindable<WeakReference<ArchiveDownloadRequest<RealmBeatmapSet>>> DownloadBegan => beatmapModelDownloader.DownloadBegan;

        public IBindable<WeakReference<ArchiveDownloadRequest<RealmBeatmapSet>>> DownloadFailed => beatmapModelDownloader.DownloadFailed;

        public bool Download(RealmBeatmapSet model, bool minimiseDownloadSize)
        {
            return beatmapModelDownloader.Download(model, minimiseDownloadSize);
        }

        public ArchiveDownloadRequest<RealmBeatmapSet> GetExistingDownload(RealmBeatmapSet model)
        {
            return beatmapModelDownloader.GetExistingDownload(model);
        }

        #endregion

        #region Implementation of IModelManager<RealmBeatmapSet>

        public IBindable<WeakReference<RealmBeatmapSet>> ItemUpdated => beatmapModelManager.ItemUpdated;

        public IBindable<WeakReference<RealmBeatmapSet>> ItemRemoved => beatmapModelManager.ItemRemoved;

        public Task ImportFromStableAsync(StableStorage stableStorage)
        {
            return beatmapModelManager.ImportFromStableAsync(stableStorage);
        }

        public void Export(RealmBeatmapSet item)
        {
            beatmapModelManager.Export(item);
        }

        public void ExportModelTo(RealmBeatmapSet model, Stream outputStream)
        {
            beatmapModelManager.ExportModelTo(model, outputStream);
        }

        public void Update(RealmBeatmapSet item)
        {
            beatmapModelManager.Update(item);
        }

        public bool Delete(RealmBeatmapSet item)
        {
            return beatmapModelManager.Delete(item);
        }

        public void Delete(List<RealmBeatmapSet> items, bool silent = false)
        {
            beatmapModelManager.Delete(items, silent);
        }

        public void Undelete(List<RealmBeatmapSet> items, bool silent = false)
        {
            beatmapModelManager.Undelete(items, silent);
        }

        public void Undelete(RealmBeatmapSet item)
        {
            beatmapModelManager.Undelete(item);
        }

        public bool IsAvailableLocally(RealmBeatmapSet model)
        {
            return beatmapModelManager.IsAvailableLocally(model);
        }

        #endregion

        #region Implementation of IWorkingBeatmapCache

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo)
        {
            return workingBeatmapCache.GetWorkingBeatmap(beatmapInfo);
        }

        public void Invalidate(BeatmapSetInfo beatmapSetInfo)
        {
            workingBeatmapCache.Invalidate(beatmapSetInfo);
        }

        public void Invalidate(BeatmapInfo beatmapInfo)
        {
            workingBeatmapCache.Invalidate(beatmapInfo);
        }

        #endregion

        #region Implementation of IModelFileManager<in RealmBeatmapSet,in BeatmapSetFileInfo>

        public void ReplaceFile(RealmBeatmapSet model, BeatmapSetFileInfo file, Stream contents, string filename)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(RealmBeatmapSet model, BeatmapSetFileInfo file)
        {
            throw new NotImplementedException();
        }

        public void AddFile(RealmBeatmapSet model, Stream contents, string filename)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            onlineBeatmapLookupQueue?.Dispose();
        }

        #endregion

        #region Implementation of IPostImports<out RealmBeatmapSet>

        public Action<IEnumerable<ILive<RealmBeatmapSet>>>? PostImport
        {
            set => beatmapImporter.PostImport = value;
        }

        #endregion

        #region Implementation of IBeatmapManager<RealmBeatmapSet>

        public List<RealmBeatmapSet> GetAllUsableBeatmapSets(IncludedDetails includes = IncludedDetails.All, bool includeProtected = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RealmBeatmapSet> GetAllUsableBeatmapSetsEnumerable(IncludedDetails includes, bool includeProtected = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RealmBeatmapSet> QueryBeatmapSets(Expression<Func<RealmBeatmapSet, bool>> query, IncludedDetails includes = IncludedDetails.All)
        {
            throw new NotImplementedException();
        }

        public RealmBeatmapSet QueryBeatmapSet(Expression<Func<RealmBeatmapSet, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query)
        {
            throw new NotImplementedException();
        }

        public BeatmapInfo QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query)
        {
            throw new NotImplementedException();
        }

        public void Hide(BeatmapInfo beatmapInfo)
        {
            throw new NotImplementedException();
        }

        public void Restore(BeatmapInfo beatmapInfo)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
