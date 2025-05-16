// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Utils;

namespace osu.Game.Screens.SelectV2
{
    public partial class SoloSongSelect : SongSelect
    {
        private PlayerLoader? playerLoader;

        public override bool EditingAllowed => true;

        protected override bool OnStart()
        {
            if (playerLoader != null) return false;

            addAutoplayIfControlPressed();

            this.Push(playerLoader = new PlayerLoader(createPlayer));
            return true;
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            revertAutoplayAfterPlay();
            playerLoader = null;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            revertAutoplayAfterPlay();
            playerLoader = null;
            return false;
        }

        private Player createPlayer()
        {
            var replayGeneratingMod = Mods.Value.OfType<ICreateReplayData>().FirstOrDefault();

            if (replayGeneratingMod != null)
                return new ReplayPlayer((beatmap, mods) => replayGeneratingMod.CreateScoreFromReplayData(beatmap, mods));

            return new SoloPlayer();
        }

        #region Handling autoplay via ctrl key

        private IReadOnlyList<Mod>? modsAtGameplayStart;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private void addAutoplayIfControlPressed()
        {
            modsAtGameplayStart = Mods.Value;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager()?.CurrentState?.Keyboard.ControlPressed == true)
            {
                var autoInstance = Ruleset.Value.CreateInstance().GetAutoplayMod();

                if (autoInstance == null)
                {
                    notifications?.Post(new SimpleNotification { Text = NotificationsStrings.NoAutoplayMod });
                    return;
                }

                Mod[] mods = Mods.Value.Append(autoInstance).ToArray();

                if (!ModUtils.CheckCompatibleSet(mods, out var invalid))
                    mods = mods.Except(invalid).Append(autoInstance).ToArray();

                Mods.Value = mods;
            }
        }

        private void revertAutoplayAfterPlay()
        {
            if (playerLoader == null) return;

            Mods.Value = modsAtGameplayStart;
        }

        #endregion

        private partial class PlayerLoader : Play.PlayerLoader
        {
            public override bool ShowFooter => true;

            public PlayerLoader(Func<Player> createPlayer)
                : base(createPlayer)
            {
            }
        }
    }
}
