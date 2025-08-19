// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using MessagePack;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the state of a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingRoomState : MatchRoomState
    {
        /// <summary>
        /// The current room status.
        /// </summary>
        [Key(0)]
        public MatchmakingRoomStatus RoomStatus { get; set; }

        /// <summary>
        /// The current round number.
        /// </summary>
        [Key(1)]
        public int Round { get; set; }

        /// <summary>
        /// The playlist items that were picked as gameplay candidates.
        /// </summary>
        [Key(2)]
        public long[] CandidateItems { get; set; } = [];

        /// <summary>
        /// The final gameplay candidate.
        /// </summary>
        [Key(3)]
        public long CandidateItem { get; set; }

        /// <summary>
        /// The users in the room.
        /// </summary>
        [Key(4)]
        public MatchmakingUserList Users { get; set; } = new MatchmakingUserList();

        /// <summary>
        /// Advances to the next round.
        /// </summary>
        public void NextRound()
        {
            Round++;
        }

        /// <summary>
        /// Sets a score for the given user in the current round.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="placement"></param>
        /// <param name="points"></param>
        /// <param name="score"></param>
        public void SetScore(int userId, int placement, int points, SoloScoreInfo score)
        {
            MatchmakingUser mmUser = Users[userId];
            mmUser.Points += points;

            MatchmakingRound mmRound = mmUser.Rounds[Round];
            mmRound.Placement = placement;
            mmRound.TotalScore = score.TotalScore;
            mmRound.Accuracy = score.Accuracy;
            mmRound.MaxCombo = score.MaxCombo;
            mmRound.Statistics = score.Statistics;
        }

        /// <summary>
        /// Computes the aggregate user placements.
        /// </summary>
        public void ComputePlacements()
        {
            int i = 1;
            foreach (var user in Users.UserDictionary.Values.Order(new MatchmakingUserComparer()))
                user.Placement = i++;
        }
    }
}
