// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick
{
    public class PickScreen : MatchmakingSubScreen
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private FillFlowContainer<BeatmapPanel> panels = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = panels = new FillFlowContainer<BeatmapPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(20, 20)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.ItemAdded += onItemAdded;
            client.ItemChanged += onItemChanged;
            client.ItemRemoved += onItemRemoved;

            foreach (var item in client.Room!.Playlist)
                onItemAdded(item);
        }

        private void onItemAdded(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            var panel = new BeatmapPanel(item)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre
            };

            panels.Add(panel);
            panels.SetLayoutPosition(panel, (float)item.StarRating);
        });

        private void onItemChanged(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            if (item.Expired)
                panels.RemoveAll(p => p.Item.ID == item.ID, true);
        });

        private void onItemRemoved(long itemId) => Scheduler.Add(() =>
        {
            panels.RemoveAll(p => p.Item.ID == itemId, true);
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.ItemAdded -= onItemAdded;
                client.ItemChanged -= onItemChanged;
                client.ItemRemoved -= onItemRemoved;
            }
        }
    }
}
