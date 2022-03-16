// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;

namespace osu.Game.Skinning
{
    public class LegacyDatabasedSkinResourceStore : ResourceStore<byte[]>
    {
        private readonly RealmAccess realm;
        private readonly Dictionary<string, string> fileToStoragePathMapping = new Dictionary<string, string>();
        private readonly Live<SkinInfo> skin;

        public LegacyDatabasedSkinResourceStore(SkinInfo source, IResourceStore<byte[]> underlyingStore, RealmAccess realm)
            : base(underlyingStore)
        {
            this.realm = realm;

            skin = source.ToLive(realm);
        }

        private void initialiseFileCache(IList<RealmNamedFileUsage> files)
        {
            fileToStoragePathMapping.Clear();
            foreach (var f in files)
                fileToStoragePathMapping[f.Filename.ToLowerInvariant()] = f.File.GetStoragePath();
        }

        protected override IEnumerable<string> GetFilenames(string name)
        {
            foreach (string filename in base.GetFilenames(name))
            {
                string path = getPathForFile(filename.ToStandardisedPath());
                if (path != null)
                    yield return path;
            }
        }

        private string getPathForFile(string filename) =>
            skin.PerformRead(s =>
            {
                return s.Files.FirstOrDefault(f => f.Filename == filename)?.File.GetStoragePath();
            });

        public override IEnumerable<string> GetAvailableResources() => fileToStoragePathMapping.Keys;
    }
}
