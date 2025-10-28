// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.OnlinePlay.Matchmaking.Intro;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    /// <summary>
    /// A component which acts as a bridge between the online component (ie <see cref="MultiplayerClient"/>)
    /// and the visual representations and flow of queueing for matchmaking.
    ///
    /// Includes support for deferring to background.
    /// </summary>
    /// <remarks>
    /// This is initialised and cached in the <see cref="ScreenQueue"/> but can be used throughout the system via DI.</remarks>
    public partial class QueueController : Component
    {
        public readonly Bindable<ScreenQueue.MatchmakingScreenState> CurrentState = new Bindable<ScreenQueue.MatchmakingScreenState>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private BackgroundQueueNotification? backgroundNotification;
        private bool isBackgrounded;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
        }

        public void SearchInBackground()
        {
            if (isBackgrounded)
                return;

            isBackgrounded = true;
            postNotification();
        }

        public void SearchInForeground()
        {
            if (!isBackgrounded)
                return;

            isBackgrounded = false;
            closeNotifications();
        }

        private void onRoomUpdated() => Scheduler.Add(() =>
        {
            if (client.Room == null)
                CurrentState.Value = ScreenQueue.MatchmakingScreenState.Idle;
        });

        private void onMatchmakingQueueJoined() => Scheduler.Add(() =>
        {
            CurrentState.Value = ScreenQueue.MatchmakingScreenState.Queueing;

            if (isBackgrounded)
            {
                closeNotifications();
                postNotification();
            }
        });

        private void onMatchmakingQueueLeft() => Scheduler.Add(() =>
        {
            if (CurrentState.Value != ScreenQueue.MatchmakingScreenState.InRoom)
                CurrentState.Value = ScreenQueue.MatchmakingScreenState.Idle;

            closeNotifications();
        });

        private void onMatchmakingRoomInvited() => Scheduler.Add(() =>
        {
            CurrentState.Value = ScreenQueue.MatchmakingScreenState.PendingAccept;

            if (backgroundNotification != null)
            {
                backgroundNotification.State = ProgressNotificationState.Completed;
                backgroundNotification = null;
            }
        });

        private void onMatchmakingRoomReady(long roomId, string password) => Scheduler.Add(() =>
        {
            client.JoinRoom(new Room { RoomID = roomId }, password)
                  .FireAndForget(() => Scheduler.Add(() =>
                  {
                      CurrentState.Value = ScreenQueue.MatchmakingScreenState.InRoom;
                  }));
        });

        private void postNotification()
        {
            if (backgroundNotification != null)
                return;

            notifications?.Post(backgroundNotification = new BackgroundQueueNotification());
        }

        private void closeNotifications()
        {
            if (backgroundNotification != null)
            {
                backgroundNotification.State = ProgressNotificationState.Cancelled;
                backgroundNotification.CloseAll();
                backgroundNotification = null;
            }
        }

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

        internal class BackgroundQueueNotification : ProgressNotification
        {
            [Resolved]
            private IPerformFromScreenRunner? performer { get; set; }

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private Notification? readyNotification;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = "Searching for opponents...";
                CompletionText = "Your match is ready! Click to join.";

                CompletionClickAction = () =>
                {
                    client.MatchmakingAcceptInvitation().FireAndForget();
                    performer?.PerformFromScreen(s => s.Push(new IntroScreen()));

                    Close(false);
                    return true;
                };

                CancelRequested = () =>
                {
                    client.MatchmakingLeaveQueue().FireAndForget();
                    return true;
                };
            }

            protected override Notification CreateCompletionNotification() =>
                readyNotification = base.CreateCompletionNotification();

            public void CloseAll()
            {
                readyNotification?.Close(false);
                Close(false);
            }
        }
    }
}
