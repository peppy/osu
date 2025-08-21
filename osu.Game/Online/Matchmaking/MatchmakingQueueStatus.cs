// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Union(0, typeof(Searching))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    [Union(1, typeof(MatchFound))]
    [Union(2, typeof(JoiningMatch))]
    public abstract class MatchmakingQueueStatus
    {
        [MessagePackObject]
        public class Searching : MatchmakingQueueStatus
        {
            [Key(0)]
            public int[] UsersInQueue { get; set; } = [];
        }

        [MessagePackObject]
        public class MatchFound : MatchmakingQueueStatus
        {
        }

        [MessagePackObject]
        public class JoiningMatch : MatchmakingQueueStatus
        {
        }
    }
}
