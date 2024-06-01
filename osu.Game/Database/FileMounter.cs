// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public class FileMounter(RealmAccess realmAccess, Storage storage, BeatmapManager beatmapManager)
    {
        private readonly BeatmapManager beatmapManager = beatmapManager;
        private readonly Storage storage = storage.GetStorageForDirectory(@"files");
        private readonly string tempDirectory = Path.GetTempPath();

        /// <summary>
        /// Mount all of the files for a <see cref="BeatmapSetInfo"/> to a temporary directory and open it in the native file explorer.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to mount</param>
        public void MountBeatmapSet(BeatmapSetInfo beatmapSetInfo)
        {
            string beatmapSetDirectory = Path.Join(tempDirectory, beatmapSetInfo.Metadata.Title);

            if (Directory.Exists(beatmapSetDirectory)) Directory.Delete(beatmapSetDirectory, true);
            Directory.CreateDirectory(beatmapSetDirectory);

            foreach (var realmFile in beatmapSetInfo.Files)
            {
                string fullPath = storage.GetFullPath(realmFile.File.GetStoragePath());
                Logger.Log("Mounting file " + fullPath);

                string destination = Path.Join(beatmapSetDirectory, realmFile.Filename);
                string? destinationDirectory = Path.GetDirectoryName(destination);

                if (destinationDirectory != null)
                    Directory.CreateDirectory(destinationDirectory);
                File.Copy(fullPath, destination, true);
            }

            Logger.Log("Beatmap set mounted");
            Process.Start(new ProcessStartInfo
            {
                FileName = beatmapSetDirectory,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Alias for <see cref="SyncBeatmapSet(BeatmapSetInfo, string?, bool)"/> with deleteFolder set to <c>true</c>.
        /// </summary>
        /// <param name="beatmapSetInfo"></param>
        public void DismountBeatmapSet(BeatmapSetInfo beatmapSetInfo) => SyncBeatmapSet(beatmapSetInfo, null, true);

        /// <summary>
        /// Replace the files in a <see cref="BeatmapSetInfo"/> with the files in a directory
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to replace the files of</param>
        /// <param name="beatmapSetDirectory">The directory with the new files</param>
        /// <param name="deleteFolder">Delete <paramref name="beatmapSetDirectory"/> after syncing</param>
        public void SyncBeatmapSet(BeatmapSetInfo beatmapSetInfo, string? beatmapSetDirectory, bool deleteFolder = false)
        {
            beatmapSetDirectory ??= Path.Join(tempDirectory, beatmapSetInfo.Metadata.Title);
            if (!Directory.Exists(beatmapSetDirectory)) throw new DirectoryNotFoundException($"Directory {beatmapSetDirectory} does not exist");

            realmAccess.Write(async realm =>
            {
                BeatmapImporter beatmapImporter = new BeatmapImporter(storage, realmAccess);

                await beatmapImporter.ImportAsUpdate(new ProgressNotification(), new ImportTask(beatmapSetDirectory), beatmapSetInfo).ConfigureAwait(false);

                if (!deleteFolder) return;

                Directory.Delete(beatmapSetDirectory, true);
                Logger.Log("Beatmap set dismounted");
            });
        }
    }
}
