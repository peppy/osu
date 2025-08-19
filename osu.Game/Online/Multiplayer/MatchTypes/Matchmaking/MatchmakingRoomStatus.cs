// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the current status of a matchmaking room.
    /// </summary>
    [Serializable]
    public enum MatchmakingRoomStatus
    {
        /// <summary>
        /// Room starts. Some players may still be joining.
        /// </summary>
        RoomStart,

        /// <summary>
        /// Round starts.
        /// </summary>
        RoundStart,

        /// <summary>
        /// Users pick beatmaps.
        /// </summary>
        UserPicks,

        /// <summary>
        /// Servers selects the next beatmap.
        /// </summary>
        SelectBeatmap,

        /// <summary>
        /// Wait for players to download + set the beatmap.
        /// </summary>
        PrepareBeatmap,

        /// <summary>
        /// Wait for players to preview the beatmap before gameplay.
        /// </summary>
        PrepareGameplay,

        /// <summary>
        /// Gameplay starts.
        /// </summary>
        Gameplay,

        /// <summary>
        /// Round ends. Some players may be viewing results.
        /// </summary>
        RoundEnd,

        /// <summary>
        /// Room ends. Some players may still be chatting.
        /// </summary>
        RoomEnd
    }
}
