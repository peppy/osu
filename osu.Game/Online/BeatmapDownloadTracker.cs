// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    public class BeatmapDownloadTracker : DownloadTracker<IBeatmapSetInfo>
    {
        [Resolved(CanBeNull = true)]
        protected BeatmapModelDownloader? Downloader { get; private set; }

        private ArchiveDownloadRequest<IBeatmapSetInfo>? attachedRequest;

        private IDisposable? realmSubscription;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public BeatmapDownloadTracker(IBeatmapSetInfo trackedItem)
            : base(trackedItem)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Downloader == null)
                return;

            Downloader.DownloadBegan += downloadBegan;
            Downloader.DownloadFailed += downloadFailed;

            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>().Where(s => s.OnlineID == TrackedItem.OnlineID && !s.DeletePending), (items, _, _) =>
            {
                bool isPresent = items.Any();

                // TODO: expose via another bindable?
                bool isUpToDate = isPresent && items.All(s => s.Beatmaps.All(b => b.MatchesOnlineVersion));

                Scheduler.AddOnce(() =>
                {
                    if (attachedRequest != null)
                        return;

                    if (isPresent)
                        UpdateState(DownloadState.LocallyAvailable);
                    else
                    {
                        UpdateState(DownloadState.NotDownloaded);

                        var existing = Downloader.GetExistingDownload(TrackedItem);
                        if (existing != null)
                            attachDownload(existing);
                    }
                });
            });
        }

        private void downloadBegan(ArchiveDownloadRequest<IBeatmapSetInfo> request) => Schedule(() =>
        {
            if (!checkEquality(request.Model, TrackedItem)) return;

            attachDownload(request);
        });

        private void onRequestSuccess(string _) => Schedule(() =>
        {
            UpdateState(DownloadState.Importing);
            clearAttachedDownload();
        });

        private void onRequestProgress(float progress) => Schedule(() => UpdateProgress(progress));

        private void downloadFailed(ArchiveDownloadRequest<IBeatmapSetInfo> request) => Schedule(() =>
        {
            if (!checkEquality(request.Model, TrackedItem)) return;

            UpdateState(DownloadState.NotDownloaded);
            clearAttachedDownload();
        });

        private void onRequestFailure(Exception e) => Schedule(() =>
        {
            UpdateState(DownloadState.NotDownloaded);
            clearAttachedDownload();
        });

        private void attachDownload(ArchiveDownloadRequest<IBeatmapSetInfo> request)
        {
            clearAttachedDownload();

            attachedRequest = request;

            attachedRequest.Failure += onRequestFailure;
            attachedRequest.DownloadProgressed += onRequestProgress;
            attachedRequest.Success += onRequestSuccess;

            // If the incoming request is already in a completed state, don't bother attaching.
            if (attachedRequest.Progress == 1)
            {
                UpdateProgress(1);
                UpdateState(DownloadState.Importing);
                clearAttachedDownload();
                return;
            }

            UpdateProgress(attachedRequest.Progress);
            UpdateState(DownloadState.Downloading);
        }

        private void clearAttachedDownload()
        {
            if (attachedRequest == null)
                return;

            attachedRequest.Failure -= onRequestFailure;
            attachedRequest.DownloadProgressed -= onRequestProgress;
            attachedRequest.Success -= onRequestSuccess;
            attachedRequest = null;
        }

        private bool checkEquality(IBeatmapSetInfo x, IBeatmapSetInfo y) => x.OnlineID == y.OnlineID;

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            clearAttachedDownload();

            realmSubscription?.Dispose();

            if (Downloader != null)
            {
                Downloader.DownloadBegan -= downloadBegan;
                Downloader.DownloadFailed -= downloadFailed;
            }
        }

        #endregion
    }
}
