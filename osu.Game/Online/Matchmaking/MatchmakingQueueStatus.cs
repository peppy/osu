// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Union(0, typeof(InQueue))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    [Union(1, typeof(FoundMatch))]
    public abstract class MatchmakingQueueStatus
    {
        [MessagePackObject]
        public class FoundMatch : MatchmakingQueueStatus
        {
            /// <summary>
            /// The ID of the room to join.
            /// </summary>
            [Key(0)]
            public long RoomId { get; set; }
        }

        [MessagePackObject]
        public class InQueue : MatchmakingQueueStatus
        {
            /// <summary>
            /// The number of players found.
            /// </summary>
            [Key(0)]
            public int PlayerCount { get; set; }

            /// <summary>
            /// The total room size.
            /// </summary>
            [Key(1)]
            public int RoomSize { get; set; }
        }
    }
}
