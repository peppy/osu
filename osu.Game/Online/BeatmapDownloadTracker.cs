// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API;
using Realms;

#nullable enable

namespace osu.Game.Online
{
    public class BeatmapDownloadTracker : DownloadTracker<IBeatmapSetInfo>
    {
        [Resolved(CanBeNull = true)]
        private IModelDownloader<IBeatmapSetInfo>? beatmapDownloader { get; set; }

        [Resolved]
        private RealmContextFactory? realmFactory { get; set; }

        private ArchiveDownloadRequest<IBeatmapSetInfo>? attachedRequest;

        private IDisposable? importCheckSubscription;

        public BeatmapDownloadTracker(IBeatmapSetInfo trackedItem)
            : base(trackedItem)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (realmFactory == null || beatmapDownloader == null) return;

            var matching = realmFactory.Context.All<RealmBeatmapSet>().Where(s => s.OnlineID == TrackedItem.OnlineID && !s.DeletePending);

            if (matching.Any())
                UpdateState(DownloadState.LocallyAvailable);
            else
                attachDownload(beatmapDownloader.GetExistingDownload(TrackedItem));

            beatmapDownloader.DownloadBegan += downloadBegan;
            beatmapDownloader.DownloadFailed += downloadFailed;

            importCheckSubscription = matching.SubscribeForNotifications(matchingChanged);
        }

        private void matchingChanged(IRealmCollection<RealmBeatmapSet> sender, ChangeSet? changes, Exception? error)
        {
            if (changes == null)
                return;

            if (changes.DeletedIndices.Any() && State.Value == DownloadState.LocallyAvailable)
                UpdateState(DownloadState.NotDownloaded);
            else if (changes.InsertedIndices.Any())
                UpdateState(DownloadState.LocallyAvailable);
        }

        private void downloadBegan(ArchiveDownloadRequest<IBeatmapSetInfo> request) => Schedule(() =>
        {
            if (checkEquality(request.Model, TrackedItem))
                attachDownload(request);
        });

        private void downloadFailed(ArchiveDownloadRequest<IBeatmapSetInfo> request) => Schedule(() =>
        {
            if (checkEquality(request.Model, TrackedItem))
                attachDownload(null);
        });

        private void attachDownload(ArchiveDownloadRequest<IBeatmapSetInfo>? request)
        {
            if (attachedRequest != null)
            {
                attachedRequest.Failure -= onRequestFailure;
                attachedRequest.DownloadProgressed -= onRequestProgress;
                attachedRequest.Success -= onRequestSuccess;
            }

            attachedRequest = request;

            if (attachedRequest != null)
            {
                if (attachedRequest.Progress == 1)
                {
                    UpdateProgress(1);
                    UpdateState(DownloadState.Importing);
                }
                else
                {
                    UpdateProgress(attachedRequest.Progress);
                    UpdateState(DownloadState.Downloading);

                    attachedRequest.Failure += onRequestFailure;
                    attachedRequest.DownloadProgressed += onRequestProgress;
                    attachedRequest.Success += onRequestSuccess;
                }
            }
            else
            {
                UpdateState(DownloadState.NotDownloaded);
            }
        }

        private void onRequestSuccess(string _) => Schedule(() => UpdateState(DownloadState.Importing));

        private void onRequestProgress(float progress) => Schedule(() => UpdateProgress(progress));

        private void onRequestFailure(Exception e) => Schedule(() => attachDownload(null));

        private void itemUpdated(BeatmapSetInfo item) => Schedule(() =>
        {
            if (checkEquality(item, TrackedItem))
                UpdateState(DownloadState.LocallyAvailable);
        });

        private void itemRemoved(BeatmapSetInfo item) => Schedule(() =>
        {
            if (checkEquality(item, TrackedItem))
                UpdateState(DownloadState.NotDownloaded);
        });

        private bool checkEquality(IBeatmapSetInfo x, IBeatmapSetInfo y) => x.OnlineID == y.OnlineID;

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            attachDownload(null);

            if (beatmapDownloader != null)
            {
                beatmapDownloader.DownloadBegan -= downloadBegan;
                beatmapDownloader.DownloadFailed -= downloadFailed;
            }

            importCheckSubscription?.Dispose();
        }

        #endregion
    }
}
