// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public class MatchmakingQueueNotification : ProgressNotification
    {
        [Resolved]
        private MatchmakingController controller { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public MatchmakingQueueNotification()
        {
            Text = "Searching for opponents...";

            CancelRequested = () =>
            {
                client.LeaveMatchmakingQueue().FireAndForget();
                return true;
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controller.CloseNotification += onCloseNotification;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
        }

        private void onCloseNotification()
        {
            State = ProgressNotificationState.Cancelled;
            Close(false);
        }

        private void onMatchmakingQueueJoined()
        {
            onCloseNotification();
        }

        private void onMatchmakingQueueLeft()
        {
            onCloseNotification();
        }

        private void onMatchmakingRoomInvited()
        {
            State = ProgressNotificationState.Completed;
        }

        protected override Notification CreateCompletionNotification() => new MatchmakingRoomReadyNotification();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (controller.IsNotNull())
                controller.CloseNotification -= onCloseNotification;

            if (client.IsNotNull())
            {
                client.MatchmakingQueueJoined -= onMatchmakingQueueJoined;
                client.MatchmakingQueueLeft -= onMatchmakingQueueLeft;
                client.MatchmakingRoomInvited -= onMatchmakingRoomInvited;
            }
        }

        private class MatchmakingRoomReadyNotification : ProgressCompletionNotification
        {
            [Resolved]
            private MatchmakingController controller { get; set; } = null!;

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            [Resolved]
            private IPerformFromScreenRunner performer { get; set; } = null!;

            public MatchmakingRoomReadyNotification()
            {
                Text = "Your match is ready! Click to join.";

                Activated += () =>
                {
                    client.MatchmakingAcceptInvitation().FireAndForget();
                    performer.PerformFromScreen(s => s.Push(new MatchmakingIntroScreen()));
                    return true;
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                controller.CloseNotification += onCloseNotification;
                client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
                client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            }

            private void onCloseNotification()
            {
                Close(false);
            }

            private void onMatchmakingQueueJoined()
            {
                onCloseNotification();
            }

            private void onMatchmakingQueueLeft()
            {
                onCloseNotification();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (controller.IsNotNull())
                    controller.CloseNotification -= onCloseNotification;

                if (client.IsNotNull())
                {
                    client.MatchmakingQueueJoined -= onMatchmakingQueueJoined;
                    client.MatchmakingQueueLeft -= onMatchmakingQueueLeft;
                }
            }
        }
    }
}
