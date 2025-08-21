// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class BeatmapSelectionGrid : CompositeDrawable
    {
        public event Action<APIBeatmap>? BeatmapClicked;

        private readonly IReadOnlyList<APIBeatmap> beatmaps;
        private readonly Dictionary<int, APIBeatmap> userSelections = new Dictionary<int, APIBeatmap>();
        private readonly Dictionary<int, BeatmapSelectionPanel> beatmapPanels = new Dictionary<int, BeatmapSelectionPanel>();

        private readonly FillFlowContainer panelFlow;

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        public BeatmapSelectionGrid(IReadOnlyList<APIBeatmap> beatmaps)
        {
            this.beatmaps = beatmaps;

            AddInternal(panelFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var beatmap in beatmaps)
            {
                var panel = new BeatmapSelectionPanel(300, 70)
                {
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.LightSlateGray,
                        }
                    },
                    Clicked = () => BeatmapClicked?.Invoke(beatmap),
                };

                panelFlow.Add(beatmapPanels[beatmap.OnlineID] = panel);
            }
        }

        public void SetUserSelection(APIUser user, APIBeatmap? beatmap, bool self = false)
        {
            try
            {
                if (userSelections.TryGetValue(user.Id, out var oldBeatmap))
                {
                    if (oldBeatmap.OnlineID != beatmap?.OnlineID)
                    {
                        beatmapPanels[oldBeatmap.OnlineID].RemoveUser(user);
                    }
                    else
                    {
                        return;
                    }
                }

                if (beatmap != null)
                    beatmapPanels[beatmap.OnlineID].AddUser(user, self);
            }
            finally
            {
                if (beatmap != null)
                    userSelections[user.Id] = beatmap;
                else
                    userSelections.Remove(user.Id);
            }
        }
    }
}
