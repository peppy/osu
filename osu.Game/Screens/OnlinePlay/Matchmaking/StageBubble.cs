// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    internal partial class StageBubble : CompositeDrawable
    {
        private readonly Color4 backgroundColour = Color4.Salmon;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly MatchmakingStage trackedStatus;

        private readonly LocalisableString displayText;
        private Drawable progressBar = null!;

        private DateTimeOffset countdownStartTime;
        private DateTimeOffset countdownEndTime;
        private SpriteIcon arrow = null!;

        public StageBubble(MatchmakingStage trackedStatus, LocalisableString displayText)
        {
            this.trackedStatus = trackedStatus;
            this.displayText = displayText;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    arrow = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Alpha = 0.5f,
                        Size = new Vector2(16),
                        Icon = FontAwesome.Solid.ArrowRight,
                        Margin = new MarginPadding { Horizontal = 10 }
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = backgroundColour.Darken(0.2f)
                            },
                            progressBar = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = backgroundColour
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = displayText,
                                Padding = new MarginPadding(10)
                            }
                        }
                    }
                }
            };

            Alpha = 0.5f;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            client.CountdownStarted += onCountdownStarted;
            client.CountdownStopped += onCountdownStopped;

            if (client.Room != null)
            {
                onMatchRoomStateChanged(client.Room.MatchState);
                foreach (var countdown in client.Room.ActiveCountdowns)
                    onCountdownStarted(countdown);
            }
        }

        protected override void Update()
        {
            base.Update();

            TimeSpan duration = countdownEndTime - countdownStartTime;

            if (duration.TotalMilliseconds == 0)
                progressBar.Width = 0;
            else
            {
                TimeSpan elapsed = DateTimeOffset.Now - countdownStartTime;
                progressBar.Width = (float)(elapsed.TotalMilliseconds / duration.TotalMilliseconds);
            }
        }

        private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState roomState)
                return;

            bool isPreparing =
                (trackedStatus == MatchmakingStage.RoundWarmupTime && roomState.Stage == MatchmakingStage.WaitingForClientsJoin) ||
                (trackedStatus == MatchmakingStage.GameplayWarmupTime && roomState.Stage == MatchmakingStage.WaitingForClientsBeatmapDownload) ||
                (trackedStatus == MatchmakingStage.ResultsDisplaying && roomState.Stage == MatchmakingStage.Gameplay);

            if (isPreparing)
            {
                arrow.FadeTo(1, 500)
                     .Then()
                     .FadeTo(0.5f, 500)
                     .Loop();
            }
        });

        private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not MatchmakingStageCountdown matchmakingState || matchmakingState.Stage != trackedStatus)
                return;

            countdownStartTime = DateTimeOffset.Now;
            countdownEndTime = countdownStartTime + countdown.TimeRemaining;
            arrow.FadeIn(500, Easing.OutQuint);
            this.FadeTo(1, 200);
        });

        private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not MatchmakingStageCountdown matchmakingStatusCountdown || matchmakingStatusCountdown.Stage != trackedStatus)
                return;

            countdownEndTime = DateTimeOffset.Now;
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchRoomStateChanged -= onMatchRoomStateChanged;
                client.CountdownStarted -= onCountdownStarted;
                client.CountdownStopped -= onCountdownStopped;
            }
        }
    }
}
