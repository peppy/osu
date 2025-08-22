// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens
{
    internal class MatchmakingQueueScreen : OsuScreen
    {
        private Container mainContent = null!;

        private MatchmakingScreenState state;
        private MatchmakingCloud cloud = null!;

        private BeatmapSelectionPanel.SelectionAvatar localUserAvatar = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                cloud = new MatchmakingCloud
                {
                    Y = -100,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.6f)
                },
                localUserAvatar = new BeatmapSelectionPanel.SelectionAvatar(api.LocalUser.Value, true)
                {
                    Y = -100,
                    Scale = new Vector2(3),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Container
                {
                    RelativePositionAxes = Axes.Y,
                    Y = 0.25f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 10f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        mainContent = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 300,
                            AutoSizeEasing = Easing.OutQuint,
                            Padding = new MarginPadding(20),
                        },
                    }
                },
            };

            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
            client.MatchmakingQueueStatusChanged += onMatchmakingQueueStatusChanged;
        }

        private void onMatchmakingQueueStatusChanged(MatchmakingQueueStatus status) => Scheduler.Add(() =>
        {
            switch (status)
            {
                case MatchmakingQueueStatus.Searching searching:
                    Users = searching.UsersInQueue.Select(u => new APIUser { Id = u }).ToArray();
                    break;
            }
        });

        private void onMatchmakingQueueJoined() => Scheduler.Add(() =>
        {
            SetState(MatchmakingScreenState.Queueing);
        });

        private void onMatchmakingRoomInvited() => Scheduler.Add(() =>
        {
            SetState(MatchmakingScreenState.PendingAccept);
        });

        private void onMatchmakingRoomReady(long roomId) => Scheduler.Add(() =>
        {
            client.JoinRoom(new Room { RoomID = roomId })
                  .FireAndForget(() => Schedule(() =>
                  {
                      SetState(MatchmakingScreenState.InRoom);

                      Scheduler.AddDelayed(() => this.Push(new MatchmakingScreen(client.Room!)), 2000);
                  }));
        });

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            using (BeginDelayedSequence(800))
            {
                Schedule(() => SetState(MatchmakingScreenState.Idle));
            }
        }

        public APIUser[] Users
        {
            set => cloud.Users = value;
        }

        public void SetState(MatchmakingScreenState newState)
        {
            state = newState;

            mainContent.FadeInFromZero(500, Easing.OutQuint);
            mainContent.Clear();

            switch (newState)
            {
                case MatchmakingScreenState.Idle:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new ShearedButton(200)
                            {
                                DarkerColour = colours.Blue2,
                                LighterColour = colours.Blue1,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = () => client.JoinMatchmakingQueue(),
                                Text = "Begin queueing",
                            }
                        }
                    };
                    break;

                case MatchmakingScreenState.Queueing:
                    ShearedButton sendToBackgroundButton;

                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Waiting for a game...",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                            new LoadingSpinner
                            {
                                State = { Value = Visibility.Visible },
                            },
                            sendToBackgroundButton = new ShearedButton(200)
                            {
                                DarkerColour = colours.Orange3,
                                LighterColour = colours.Orange4,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Queue in background",
                                Enabled = { Value = false },
                                TooltipText = "Wait 5 seconds for this option to become available."
                            }
                        }
                    };

                    Scheduler.AddDelayed(() =>
                    {
                        if (state != newState)
                            return;

                        sendToBackgroundButton.Enabled.Value = true;
                        sendToBackgroundButton.Action = this.Exit;
                        sendToBackgroundButton.TooltipText = "You will receive a notification when your game is ready. Make sure to watch out for it!";
                    }, 5000);
                    break;

                case MatchmakingScreenState.PendingAccept:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Found a match!",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Regular, typeface: Typeface.TorusAlternate),
                            },
                            new ShearedButton(200)
                            {
                                DarkerColour = colours.YellowDark,
                                LighterColour = colours.YellowLight,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = () =>
                                {
                                    client.MatchmakingAcceptInvitation()
                                          .FireAndForget(() => Schedule(() =>
                                          {
                                              SetState(MatchmakingScreenState.AcceptedWaitingForRoom);
                                          }));
                                },
                                Text = "Join match!",
                            }
                        }
                    };
                    break;

                case MatchmakingScreenState.AcceptedWaitingForRoom:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Waiting for all players...",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                            new LoadingSpinner
                            {
                                State = { Value = Visibility.Visible },
                            },
                        }
                    };
                    break;

                case MatchmakingScreenState.InRoom:
                    // room received, show users and transition to next screen.
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Good luck!",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                        }
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchmakingQueueJoined -= onMatchmakingQueueJoined;
                client.MatchmakingRoomInvited -= onMatchmakingRoomInvited;
                client.MatchmakingRoomReady -= onMatchmakingRoomReady;
                client.MatchmakingQueueStatusChanged -= onMatchmakingQueueStatusChanged;
            }
        }

        public enum MatchmakingScreenState
        {
            Idle,
            Queueing,
            PendingAccept,
            AcceptedWaitingForRoom,
            InRoom
        }
    }
}
