// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Orders <see cref="MatchmakingUser"/> in order of placement.
    /// </summary>
    public class MatchmakingUserComparer : Comparer<MatchmakingUser>
    {
        public override int Compare(MatchmakingUser? x, MatchmakingUser? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            // X appears later in the list if it has fewer points.
            if (y.Points > x.Points)
                return 1;

            // X appears earlier in the list if it has more points.
            if (x.Points > y.Points)
                return -1;

            // Tiebreaker 1 (likely): From each user's point-of-view, their earliest and best placement.
            for (int round = 1;; round++)
            {
                if (!x.Rounds.RoundsDictionary.TryGetValue(round, out MatchmakingRound? xRound))
                    break;

                if (!y.Rounds.RoundsDictionary.TryGetValue(round, out MatchmakingRound? yRound))
                    break;

                // X appears earlier in the list if it has a better placement, or later in the list if it has a worse placement.
                int compare = xRound.Placement.CompareTo(yRound.Placement);
                if (compare != 0)
                    return compare;
            }

            // Tiebreaker 2 (unlikely): User ID.
            return x.UserId.CompareTo(y.UserId);
        }
    }
}
