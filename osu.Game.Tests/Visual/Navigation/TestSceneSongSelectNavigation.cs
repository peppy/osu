// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    /// <summary>
    /// Tests copied out of `TestSceneScreenNavigation` which are specific to song select.
    /// These are for SongSelectV2. Eventually, the tests in the above class should be deleted along with old song select.
    /// </summary>
    public partial class TestSceneSongSelectNavigation : OsuGameTestScene
    {
        [Test]
        public void TestReturnToSongSelectFromResults()
        {
            BeatmapInfo? beatmap = null!;

            playToResults();

            AddStep("store current beatmap", () => beatmap = Game.Beatmap.Value.BeatmapInfo);
            AddStep("import extra beatmaps", () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Game.BeatmapManager.Import(TestResources.CreateTestBeatmapSetInfo());
                }
            });

            pushEscape();

            waitForScreen<SoloSongSelect>();
            AddAssert("beatmap didn't change", () => Game.Beatmap.Value.BeatmapInfo, () => Is.EqualTo(beatmap));

            pushEnter();

            waitForScreen<Player>();
            AddAssert("beatmap didn't change", () => Game.Beatmap.Value.BeatmapInfo, () => Is.EqualTo(beatmap));
        }

        [Test]
        public void TestRetryFromResults()
        {
            var getOriginalPlayer = playToResults();

            AddStep("attempt to retry", () => ((ResultsScreen)Game.ScreenStack.CurrentScreen).ChildrenOfType<HotkeyRetryOverlay>().First().Action());
            AddUntilStep("wait for player", () => Game.ScreenStack.CurrentScreen != getOriginalPlayer() && Game.ScreenStack.CurrentScreen is Player);
        }

        [Test]
        public void TestPushSongSelectAndPressBackButtonImmediately()
        {
            AddStep("push song select", () => Game.ScreenStack.Push(new SoloSongSelect()));

            // TODO: without this step, a critical bug will be hit, see inline comment in `OsuGame.handleBackButton`.
            AddUntilStep("Wait for song select", () => Game.ScreenStack.CurrentScreen is SoloSongSelect select && select.IsLoaded);

            AddStep("press back button", () => Game.ChildrenOfType<ScreenBackButton>().First().Action!.Invoke());

            ConfirmAtMainMenu();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestSongContinuesAfterExitPlayer(bool withUserPause)
        {
            Player? player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            if (withUserPause)
                AddStep("pause", () => Game.Dependencies.Get<MusicController>().Stop(true));

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for fail", () => player?.GameplayState.HasFailed, () => Is.True);

            AddUntilStep("wait for track stop", () => !Game.MusicController.IsPlaying);
            AddAssert("Ensure time before preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);

            pushEscape();

            AddUntilStep("wait for track playing", () => Game.MusicController.IsPlaying);
            AddAssert("Ensure time wasn't reset to preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);
        }

        private Func<Player> playToResults()
        {
            var player = playToCompletion();
            AddUntilStep("wait for results", () => (Game.ScreenStack.CurrentScreen as ResultsScreen)?.IsLoaded == true);
            return player;
        }

        private Func<Player> playToCompletion()
        {
            Player? player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("set mods", () => Game.SelectedMods.Value = new Mod[] { new OsuModNoFail(), new OsuModDoubleTime { SpeedChange = { Value = 2 } } });

            pushEnter();

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for track playing", () => beatmap().Track.IsRunning);
            AddStep("seek to near end", () => player.ChildrenOfType<GameplayClockContainer>().First().Seek(beatmap().Beatmap.HitObjects[^1].StartTime - 1000));
            AddUntilStep("wait for complete", () => player?.GameplayState.HasPassed, () => Is.True);

            return () => player!;
        }

        private void waitForScreen<T>() where T : OsuScreen =>
            AddUntilStep($"Wait for {typeof(T).ReadableName()}", () => Game.ScreenStack.CurrentScreen is T screen && screen.IsLoaded);

        private void pushEnter() =>
            AddStep("Press enter", () => InputManager.Key(Key.Enter));

        private void pushEscape() =>
            AddStep("Press escape", () => InputManager.Key(Key.Escape));

        private void exitViaEscapeAndConfirm()
        {
            pushEscape();
            ConfirmAtMainMenu();
        }

        private void exitViaBackButtonAndConfirm()
        {
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));
            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            ConfirmAtMainMenu();
        }

        private const float click_padding = 25;

        private Vector2 backButtonPosition => Game.ToScreenSpace(new Vector2(click_padding, Game.LayoutRectangle.Bottom - click_padding));
    }
}
