// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Results;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Selection;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens
{
    public class MatchmakingScreenStack : ScreenStack
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public MatchmakingScreenStack()
        {
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Push(new IdleScreen());

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            onMatchRoomStateChanged(client.Room!.MatchState);
        }

        private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState)
                return;

            switch (matchmakingState.RoomStatus)
            {
                case MatchmakingRoomStatus.RoomStart:
                case MatchmakingRoomStatus.RoundStart:
                case MatchmakingRoomStatus.RoundEnd:
                    while (CurrentScreen is not IdleScreen)
                        Exit();
                    break;

                case MatchmakingRoomStatus.UserPicks:
                    Push(new PickScreen());
                    break;

                case MatchmakingRoomStatus.SelectBeatmap:
                    MultiplayerPlaylistItem[] candidateItems = matchmakingState.CandidateItems.Select(item => client.Room!.Playlist.Single(i => i.ID == item)).ToArray();
                    MultiplayerPlaylistItem finalItem = client.Room!.Playlist.Single(i => i.ID == matchmakingState.CandidateItem);
                    Push(new SelectionScreen(candidateItems, finalItem));
                    break;

                case MatchmakingRoomStatus.RoomEnd:
                    Push(new ResultsScreen());
                    break;
            }
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchRoomStateChanged -= onMatchRoomStateChanged;
        }
    }
}
