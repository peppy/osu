// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    public class RealmBeatmapManager : BeatmapManager
    {
        [UsedImplicitly]
        private readonly RealmContextFactory realmFactory;

        public RealmBeatmapManager(Storage storage, RealmContextFactory realmFactory, IDatabaseContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, [NotNull] AudioManager audioManager, IResourceStore<byte[]> resources, GameHost host = null, WorkingBeatmap defaultBeatmap = null, bool performOnlineLookups = false)
            : base(storage, contextFactory, rulesets, api, audioManager, resources, host, defaultBeatmap, performOnlineLookups)
        {
            this.realmFactory = realmFactory;
        }
    }
}
