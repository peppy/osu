// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Selection;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public class TestSceneSelectionScreen : MultiplayerTestScene
    {
        [Test]
        public void TestScrollManyItems()
        {
            AddStep("scroll", () =>
            {
                MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 50).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                }).ToArray();

                Child = new ScreenStack(new SelectionScreen(beatmaps, beatmaps[0]))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f),
                };
            });
        }

        [Test]
        public void TestScrollFewItems()
        {
            AddStep("scroll", () =>
            {
                MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 3).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                }).ToArray();

                Child = new ScreenStack(new SelectionScreen(beatmaps, beatmaps[0]))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f),
                };
            });
        }

        [Test]
        public void TestScrollOneItem()
        {
            AddStep("scroll", () =>
            {
                MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 1).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                }).ToArray();

                Child = new ScreenStack(new SelectionScreen(beatmaps, beatmaps[0]))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f),
                };
            });
        }
    }
}
