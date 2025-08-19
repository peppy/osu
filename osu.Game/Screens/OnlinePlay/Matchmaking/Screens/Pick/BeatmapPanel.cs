// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick
{
    public class BeatmapPanel : CompositeDrawable
    {
        private const int panel_width = 300;
        private readonly Color4 backgroundColour = OsuColour.Gray(0.3f);
        private readonly Color4 hoverColour = OsuColour.Gray(0.4f);
        private readonly Color4 clickColour = OsuColour.Gray(0.6f);

        public bool AllowSelection { get; set; } = true;

        public readonly MultiplayerPlaylistItem Item;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private Drawable background = null!;
        private FillFlowContainer<SelectionBadge> badges = null!;

        public BeatmapPanel(MultiplayerPlaylistItem item)
        {
            Item = item;
            Size = new Vector2(panel_width, 50);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapLookupCache beatmapLookupCache)
        {
            LinkFlowContainer beatmapText;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 16,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colours.ForStarDifficulty(Item.StarRating)
                            },
                            new StarRatingDisplay(new StarDifficulty(Item.StarRating, 0), StarRatingDisplaySize.Small)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },
                        }
                    },
                    beatmapText = new LinkFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        MaximumSize = new Vector2(panel_width, 100),
                        Margin = new MarginPadding { Top = 18 }
                    },
                    badges = new FillFlowContainer<SelectionBadge>
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Margin = new MarginPadding(2),
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(2),
                    }
                }
            };

            beatmapLookupCache.GetBeatmapAsync(Item.BeatmapID).ContinueWith(b => Schedule(() =>
            {
                APIBeatmap beatmap = b.GetResultSafely()!;
                beatmapText.AddLink(beatmap.GetDisplayTitleRomanisable(includeCreator: false), LinkAction.OpenBeatmap, beatmap.OnlineID.ToString());
            }));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchmakingSelectionToggled += onSelectionToggled;
        }

        private void onSelectionToggled(int userId, long playlistItemId)
        {
            if (Item.ID != playlistItemId)
                return;

            Scheduler.Add(() =>
            {
                SelectionBadge? existing = badges.SingleOrDefault(b => b.UserId == userId);

                if (existing != null)
                    existing.Expire();
                else
                    badges.Add(new SelectionBadge(userId));
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!AllowSelection)
                return false;

            background.FadeColour(hoverColour, 200);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!AllowSelection)
                return;

            background.FadeColour(backgroundColour, 100);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!AllowSelection)
                return false;

            background.FlashColour(clickColour, 200, Easing.OutQuint);
            client.MatchmakingToggleSelection(Item.ID);
            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchmakingSelectionToggled -= onSelectionToggled;
        }

        private class SelectionBadge : CompositeDrawable
        {
            public readonly int UserId;

            public SelectionBadge(int userId)
            {
                UserId = userId;
                Size = new Vector2(10);

                InternalChild = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = new UpdateableAvatar(new APIUser { Id = userId })
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                };
            }
        }
    }
}
