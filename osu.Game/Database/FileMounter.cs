// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public class FileMounter
    {
        private readonly Storage storage;

        private readonly RealmAccess realm;

        public FileMounter(RealmAccess realmAccess, Storage storage)
        {
            realm = realmAccess;
            this.storage = storage;
        }

        /// <summary>
        /// Mount all files for a <see cref="BeatmapSetInfo"/> to a temporary directory and open it in the native file explorer.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to mount</param>
        public IDisposable MountBeatmapSet(BeatmapSetInfo beatmapSetInfo)
        {
            string mountedFolder = Path.Join(Path.GetTempPath(), beatmapSetInfo.Hash);

            if (Directory.Exists(mountedFolder)) Directory.Delete(mountedFolder, true);
            Directory.CreateDirectory(mountedFolder);

            var fileStorage = storage.GetStorageForDirectory("files");

            foreach (var realmFile in beatmapSetInfo.Files)
            {
                string fullPath = fileStorage.GetFullPath(realmFile.File.GetStoragePath());
                Logger.Log("Mounting file " + fullPath);

                string destination = Path.Join(mountedFolder, realmFile.Filename);
                string? destinationDirectory = Path.GetDirectoryName(destination);

                if (destinationDirectory != null)
                    Directory.CreateDirectory(destinationDirectory);
                File.Copy(fullPath, destination, true);
            }

            Logger.Log("Beatmap set mounted");
            Process.Start(new ProcessStartInfo
            {
                FileName = mountedFolder,
                UseShellExecute = true
            });

            return new InvokeOnDisposal(() => dismountBeatmapSet(beatmapSetInfo, mountedFolder));
        }

        private void dismountBeatmapSet(BeatmapSetInfo beatmapSetInfo, string tempFolder)
        {
            if (!Directory.Exists(tempFolder))
                return;

            Task.Factory.StartNew(async () =>
            {
                BeatmapImporter beatmapImporter = new BeatmapImporter(storage, realm);

                await beatmapImporter.ImportAsUpdate(new ProgressNotification(), new ImportTask(tempFolder), beatmapSetInfo)
                                     .ConfigureAwait(false);

                Directory.Delete(tempFolder, true);
                Logger.Log("Beatmap set dismounted");
            }, TaskCreationOptions.LongRunning).WaitSafely();
        }
    }
}
