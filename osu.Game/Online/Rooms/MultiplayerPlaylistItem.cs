// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerPlaylistItem
    {
        [Key(0)]
        public long ID { get; set; }

        [Key(1)]
        public int OwnerID { get; set; }

        [Key(2)]
        public int BeatmapID { get; set; }

        [Key(3)]
        public string BeatmapChecksum { get; set; } = string.Empty;

        [Key(4)]
        public int RulesetID { get; set; }

        [Key(5)]
        public IEnumerable<APIMod> RequiredMods { get; set; } = Enumerable.Empty<APIMod>();

        [Key(6)]
        public IEnumerable<APIMod> AllowedMods { get; set; } = Enumerable.Empty<APIMod>();

        [Key(7)]
        public bool Expired { get; set; }

        /// <summary>
        /// The order in which this <see cref="MultiplayerPlaylistItem"/> will be played relative to others.
        /// Playlist items should be played in increasing order (lower values are played first).
        /// </summary>
        /// <remarks>
        /// This is only valid for items which are not <see cref="Expired"/>. The value for expired items is undefined and should not be used.
        /// </remarks>
        [Key(8)]
        public ushort PlaylistOrder { get; set; }

        /// <summary>
        /// The date when this <see cref="MultiplayerPlaylistItem"/> was played.
        /// </summary>
        [Key(9)]
        public DateTimeOffset? PlayedAt { get; set; }

        [Key(10)]
        public double StarRating { get; set; }

        /// <summary>
        /// Indicates whether participants in the room are able to pick their own choice of beatmap difficulty and ruleset.
        /// </summary>
        [Key(11)]
        public bool Freestyle { get; set; }

        /// <summary>
        /// Creates a new <see cref="MultiplayerPlaylistItem"/>.
        /// </summary>
        [SerializationConstructor]
        public MultiplayerPlaylistItem()
        {
        }

        /// <summary>
        /// Creates a new <see cref="MultiplayerPlaylistItem"/> from an API <see cref="PlaylistItem"/>.
        /// </summary>
        /// <remarks>
        /// This will create unique instances of the <see cref="RequiredMods"/> and <see cref="AllowedMods"/> arrays but NOT unique instances of the contained <see cref="APIMod"/>s.
        /// </remarks>
        public MultiplayerPlaylistItem(PlaylistItem item)
        {
            ID = item.ID;
            OwnerID = item.OwnerID;
            BeatmapID = item.Beatmap.OnlineID;
            BeatmapChecksum = item.Beatmap.MD5Hash;
            RulesetID = item.RulesetID;
            RequiredMods = item.RequiredMods.ToArray();
            AllowedMods = item.AllowedMods.ToArray();
            Expired = item.Expired;
            PlaylistOrder = item.PlaylistOrder ?? 0;
            PlayedAt = item.PlayedAt;
            StarRating = item.Beatmap.StarRating;
            Freestyle = item.Freestyle;
        }

        /// <summary>
        /// Creates a copy of this <see cref="MultiplayerPlaylistItem"/>.
        /// </summary>
        /// <remarks>
        /// This will create unique instances of the <see cref="RequiredMods"/> and <see cref="AllowedMods"/> arrays but NOT unique instances of the contained <see cref="APIMod"/>s.
        /// </remarks>
        public MultiplayerPlaylistItem Clone()
        {
            MultiplayerPlaylistItem clone = (MultiplayerPlaylistItem)MemberwiseClone();
            clone.RequiredMods = RequiredMods.ToArray();
            clone.AllowedMods = AllowedMods.ToArray();
            return clone;
        }
    }
}
