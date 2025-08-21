// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectionPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Test]
        public void TestBeatmapPanel()
        {
            BeatmapSelectionPanel panel = null!;

            AddStep("add panel", () => Child = panel = new BeatmapSelectionPanel(300, 70)
            {
                Scale = new Vector2(2),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
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
                Margin = new MarginPadding(20),
            });

            AddStep("add maarvin", () => panel.AddUser(new APIUser
            {
                Id = 6411631,
                Username = "Maarvin",
            }));
            AddStep("add peppy", () => panel.AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }));
            AddStep("add smogipoo", () => panel.AddUser(new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
            }));
            AddStep("remove smogipoo", () => panel.RemoveUser(new APIUser { Id = 1040328 }));
            AddStep("remove peppy", () => panel.RemoveUser(new APIUser { Id = 2 }));
            AddStep("remove maarvin", () => panel.RemoveUser(new APIUser { Id = 6411631 }));
        }

        [Test]
        public void TestPanelGrid()
        {
            APIBeatmap[] beatmaps = new APIBeatmap[10];
            for (int i = 0; i < beatmaps.Length; i++)
                beatmaps[i] = CreateAPIBeatmap();

            APIUser[] users = new[]
            {
                new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                },
                new APIUser
                {
                    Id = 1040328,
                    Username = "smoogipoo",
                },
                new APIUser
                {
                    Id = 6573093,
                    Username = "OliBomby",
                },
                new APIUser
                {
                    Id = 7782553,
                    Username = "aesth",
                }
            };

            RandomUserSelection randomSelection = null!;

            AddStep("add panels", () =>
            {
                var selectionGrid = new BeatmapSelectionGrid(beatmaps)
                {
                    Width = 700,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                var maarvin = new APIUser
                {
                    Id = 6411631,
                    Username = "Maarvin",
                };

                selectionGrid.BeatmapClicked += beatmap => selectionGrid.SetUserSelection(maarvin, beatmap, true);

                Children = new Drawable[]
                {
                    selectionGrid,
                    randomSelection = new RandomUserSelection(selectionGrid, users, beatmaps)
                };
            });

            AddToggleStep("random selection", enabled => randomSelection.Enabled = enabled);
        }

        private partial class RandomUserSelection(
            BeatmapSelectionGrid selection,
            APIUser[] users,
            APIBeatmap[] beatmaps
        ) : Component
        {
            public bool Enabled;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Scheduler.AddDelayed(() =>
                {
                    if (!Enabled)
                        return;

                    var user = Random.Shared.GetItems(users, 1).First();
                    var beatmap = Random.Shared.GetItems(beatmaps, 1).First();

                    selection.SetUserSelection(user, beatmap);
                }, 1000, true);
            }
        }
    }
}
