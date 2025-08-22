// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public class MatchmakingController : Component
    {
        public event Action? CloseNotification;

        public readonly Bindable<MatchmakingQueueScreen.MatchmakingScreenState> CurrentState = new Bindable<MatchmakingQueueScreen.MatchmakingScreenState>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
        }

        public void CloseAllNotifications()
        {
            CloseNotification?.Invoke();
        }

        private void onRoomUpdated() => Scheduler.Add(() =>
        {
            if (client.Room == null)
                CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.Idle;
        });

        private void onMatchmakingQueueJoined() => Scheduler.Add(() =>
        {
            CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.Queueing;
        });

        private void onMatchmakingQueueLeft() => Scheduler.Add(() =>
        {
            if (CurrentState.Value != MatchmakingQueueScreen.MatchmakingScreenState.InRoom)
                CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.Idle;
        });

        private void onMatchmakingRoomInvited() => Scheduler.Add(() =>
        {
            CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.PendingAccept;
        });

        private void onMatchmakingRoomReady(long roomId) => Scheduler.Add(() =>
        {
            client.JoinRoom(new Room { RoomID = roomId })
                  .FireAndForget(() => Scheduler.Add(() =>
                  {
                      CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.InRoom;
                  }));
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.MatchmakingQueueJoined -= onMatchmakingQueueJoined;
                client.MatchmakingQueueLeft -= onMatchmakingQueueLeft;
                client.MatchmakingRoomInvited -= onMatchmakingRoomInvited;
                client.MatchmakingRoomReady -= onMatchmakingRoomReady;
            }
        }
    }
}
