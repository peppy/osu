// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public class MatchmakingQueueBanner : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        private readonly Bindable<bool> isVisible = new Bindable<bool>();
        private readonly IBindable<bool> isConnected = new Bindable<bool>();
        private SpriteText statusText = null!;
        private Drawable background = null!;

        private MatchmakingQueueStatus? currentStatus;

        public MatchmakingQueueBanner()
        {
            AutoSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                AutoSizeAxes = Axes.X,
                AutoSizeDuration = 200,
                AutoSizeEasing = Easing.OutQuint,
                Height = 36,
                Masking = true,
                Children = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Yellow
                    },
                    statusText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding(10),
                        Colour = Color4.Black,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isVisible.BindValueChanged(onIsVisibleChanged, true);

            isConnected.BindTo(client.IsConnected);
            isConnected.BindValueChanged(onIsConnectedChanged, true);

            client.MatchmakingQueueStatusChanged += onMatchmakingQueueStatusChanged;
            onMatchmakingQueueStatusChanged(null);
        }

        private void onIsVisibleChanged(ValueChangedEvent<bool> e)
        {
            if (e.NewValue)
                statusText.BypassAutoSizeAxes = Axes.None;
            else
                statusText.BypassAutoSizeAxes = Axes.X;
        }

        private void onIsConnectedChanged(ValueChangedEvent<bool> e) => Scheduler.Add(() =>
        {
            if (!e.NewValue)
                currentStatus = null;
        });

        private void onMatchmakingQueueStatusChanged(MatchmakingQueueStatus? status) => Scheduler.Add(() =>
        {
            currentStatus = status;

            if (status == null)
            {
                Hide();
                return;
            }

            Show();

            switch (status)
            {
                case MatchmakingQueueStatus.InQueue inQueue:
                    background.Colour = Color4.Yellow;
                    statusText.Text = $"finding a match ({inQueue.PlayerCount} / {inQueue.RoomSize})...";
                    break;

                case MatchmakingQueueStatus.FoundMatch:
                    background.Colour = Color4.LightBlue;
                    statusText.Text = "match ready! click to join!";
                    break;
            }
        });

        protected override bool OnClick(ClickEvent e)
        {
            if (currentStatus is MatchmakingQueueStatus.FoundMatch found)
            {
                background.FlashColour(Color4.LightBlue.Lighten(0.5f), 100, Easing.OutQuint);

                // Perform all actions from the menu, exiting any existing multiplayer/matchmaking screen.
                performer.PerformFromScreen(_ =>
                {
                    // Now that we have a fresh slate, we can join the room.
                    client.JoinRoom(new Room { RoomID = found.RoomId })
                          .FireAndForget(() => Schedule(() => performer.PerformFromScreen(screen => screen.Push(new MatchmakingScreen(client.Room!)))));
                });

                // Immediately consume the status to ensure a secondary click doesn't attempt to re-join.
                currentStatus = null;
                Hide();
                return true;
            }

            return false;
        }

        public override void Show() => isVisible.Value = true;

        public override void Hide() => isVisible.Value = false;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchmakingQueueStatusChanged -= onMatchmakingQueueStatusChanged;
        }
    }
}
