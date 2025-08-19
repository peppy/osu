// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public class TestScenePickScreen : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom();
                room.Playlist = Enumerable.Range(1, 50).Select(i => new PlaylistItem(new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                })).ToArray();

                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("add list", () =>
            {
                Child = new ScreenStack(new PickScreen())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f)
                };
            });
        }
    }
}
