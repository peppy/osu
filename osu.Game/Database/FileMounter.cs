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

namespace osu.Game.Database
{
    public class FileMounter(RealmAccess realmAccess, Storage storage)
    {
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

            foreach (string file in Directory.GetFiles(beatmapSetDirectory))
            {
                string filename = Path.GetFileName(file);
                string fileHash = file.ComputeSHA2Hash();

                var realmFile = beatmapSetInfo.Files.FirstOrDefault(f => f.Filename == filename);

                if (realmFile == null)
                {
                    // This file is not in the database, meaning it's new. We should add it to the database.
                    Logger.Log($"File {file} is new. Adding to the database");

                    // Add the file to the storage and the database
                    using var stream = File.OpenRead(file);
                    new RealmFileStore(realmAccess, storage).Add(stream, realmAccess.Realm);
                }
                else if (fileHash != realmFile.File.Hash)
                {
                    // This file has been modified. We should replace the local file with the modified one.
                    Logger.Log($"File {file} has been modified. Replacing local file with the modified one");
                    Logger.Log("Old hash: " + realmFile.File.Hash);

                    RealmNamedFileUsage newRealmFile = new RealmNamedFileUsage(new RealmFile { Hash = fileHash }, filename);

                    realmAccess.Write(realm =>
                    {
                        // Add the new file to the storage
                        using (var stream = File.OpenRead(file)) new RealmFileStore(realmAccess, storage).Add(stream, realm);

                        beatmapSetInfo.Files.Remove(realmFile);
                        beatmapSetInfo.Files.Add(newRealmFile);

                        if (filename.EndsWith(".osu", StringComparison.Ordinal))
                        {
                            var oldBeatmap = beatmapSetInfo.Beatmaps.FirstOrDefault(f => f.Hash == realmFile.File.Hash);

                            if (oldBeatmap != null)
                            {
                                oldBeatmap.Hash = fileHash;
                            }
                        }
                    });
                    Logger.Log("New file hash: " + newRealmFile.File.Hash);
                }
                else
                {
                    Logger.Log("No changes to file " + file + " detected. Skipping");
                }
            }

            // Now do a pass of the files in the database to see if there are any files that are no longer present in the directory, which means they were deleted, and should be removed from the database.
            foreach (var file in beatmapSetInfo.Files)
            {
                // Check if the file is still present in the directory. If it is, we don't need to do anything.
                string fullPath = Path.Join(beatmapSetDirectory, file.Filename);
                if (File.Exists(fullPath)) continue;

                Logger.Log($"File {file.Filename} has been deleted. Removing from the database");

                // Remove the file from the database
                beatmapSetInfo.Files.Remove(file);
            }

            if (!deleteFolder) return;

            Directory.Delete(beatmapSetDirectory, true);
            Logger.Log("Beatmap set dismounted");
        }
    }
}
