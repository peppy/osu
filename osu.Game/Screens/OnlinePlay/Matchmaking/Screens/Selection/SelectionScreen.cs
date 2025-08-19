// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Selection
{
    public class SelectionScreen : MatchmakingSubScreen
    {
        /// <summary>
        /// Number of items visible on either side of the current item in the carousel at any one time.
        /// </summary>
        private const int visible_extent = 8;

        /// <summary>
        /// Number of cycles of the full list of items before the final item should be presented.
        /// </summary>
        private const int cycles = 5;

        private const double scroll_delay = 1000;
        private const double scroll_duration = 4000;
        private const double finish_delay = 250;
        private const double finish_duration = 1000;

        private readonly MultiplayerPlaylistItem[] candidateItems;
        private readonly MultiplayerPlaylistItem finalItem;

        private readonly Bindable<float> currentPosition = new Bindable<float>();
        private Container<BeatmapPanel> panels = null!;

        public SelectionScreen(MultiplayerPlaylistItem[] candidateItems, MultiplayerPlaylistItem finalItem)
        {
            this.candidateItems = candidateItems;
            this.finalItem = finalItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.3f,
                    }
                },
                panels = new FillFlowContainer<BeatmapPanel>
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentPosition.BindValueChanged(updatePosition, true);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            if (candidateItems.Length == 0)
                return;

            int finalPosition = cycles * candidateItems.Length + Array.FindIndex(candidateItems, i => i.Equals(finalItem));

            this.Delay(scroll_delay)
                .TransformBindableTo(currentPosition, finalPosition, scroll_duration, Easing.OutQuint)
                .Finally(_ =>
                {
                    panels[panels.Count / 2]
                        .Delay(finish_delay)
                        .ScaleTo(new Vector2(1.5f), finish_duration, Easing.OutQuint);
                });
        }

        private void updatePosition(ValueChangedEvent<float> pos)
        {
            panels.Clear();

            if (candidateItems.Length == 0)
                return;

            float centrePos = pos.NewValue % candidateItems.Length;
            int firstVisibleIndex = (int)centrePos - visible_extent;
            int lastVisibleIndex = (int)centrePos + visible_extent;

            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                int itemIndex = i;

                while (itemIndex < 0)
                    itemIndex += candidateItems.Length;

                while (itemIndex >= candidateItems.Length)
                    itemIndex -= candidateItems.Length;

                float distFromCentre = (i - centrePos) / visible_extent;

                panels.Add(new BeatmapPanel(candidateItems[itemIndex])
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AllowSelection = false,
                    Scale = new Vector2(Math.Clamp(1f - Math.Abs(distFromCentre), 0f, 1))
                });
            }
        }
    }
}
