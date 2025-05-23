// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;

namespace osu.Game.Extensions
{
    public static class StorageExtensions
    {
        /// <summary>
        /// Returns the <see cref="Storage"/> used for storing the game's
        /// </summary>
        public static Storage GetUserFileStorage(this Storage storage)
        {
            if (storage.ExistsDirectory("files"))
                return storage.GetStorageForDirectory("files");

            return storage.GetStorageForDirectory("user_data");
        }
    }
}
