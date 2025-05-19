// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Utils;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.SelectV2
{
    public partial class SoloSongSelect : SongSelect
    {
        private PlayerLoader? playerLoader;
        private IReadOnlyList<Mod>? modsAtGameplayStart;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        public override IEnumerable<OsuMenuItem> GetPrimaryActions(BeatmapInfo beatmap)
        {
            yield return new OsuMenuItem(ButtonSystemStrings.Play.ToSentence(), MenuItemType.Highlighted, () => SelectAndStart(beatmap)) { Icon = FontAwesome.Solid.Check };
            yield return new OsuMenuItem(ButtonSystemStrings.Edit.ToSentence(), MenuItemType.Standard, () => edit(beatmap)) { Icon = FontAwesome.Solid.PencilAlt };
        }

        public override IEnumerable<OsuMenuItem> GetSecondaryActions(BeatmapInfo beatmap)
        {
            foreach (var item in base.GetSecondaryActions(beatmap))
                yield return item;

            // TODO: replace with "remove from played" button when beatmap is already played.
            yield return new OsuMenuItem(SongSelectStrings.MarkAsPlayed, MenuItemType.Standard, () => beatmaps.MarkPlayed(beatmap)) { Icon = FontAwesome.Solid.TimesCircle };
            yield return new OsuMenuItem(SongSelectStrings.ClearAllLocalScores, MenuItemType.Standard, () => dialogOverlay?.Push(new BeatmapClearScoresDialog(beatmap)))
                { Icon = FontAwesome.Solid.Eraser };
            yield return new OsuMenuItem(CommonStrings.ButtonsHide.ToSentence(), MenuItemType.Destructive, () => beatmaps.Hide(beatmap));
        }

        protected override bool OnStart()
        {
            if (playerLoader != null) return false;

            modsAtGameplayStart = Mods.Value;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager()?.CurrentState?.Keyboard.ControlPressed == true)
            {
                var autoInstance = getAutoplayMod();

                if (autoInstance == null)
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = NotificationsStrings.NoAutoplayMod
                    });
                    return false;
                }

                var mods = Mods.Value.Append(autoInstance).ToArray();

                if (!ModUtils.CheckCompatibleSet(mods, out var invalid))
                    mods = mods.Except(invalid).Append(autoInstance).ToArray();

                Mods.Value = mods;
            }

            this.Push(playerLoader = new PlayerLoader(createPlayer));
            return true;

            Player createPlayer()
            {
                Player player;

                var replayGeneratingMod = Mods.Value.OfType<ICreateReplayData>().FirstOrDefault();

                if (replayGeneratingMod != null)
                {
                    player = new ReplayPlayer((beatmap, mods) => replayGeneratingMod.CreateScoreFromReplayData(beatmap, mods));
                }
                else
                {
                    player = new SoloPlayer();
                }

                return player;
            }
        }

        private void edit(BeatmapInfo beatmap)
        {
            // Forced refetch is important here to guarantee correct invalidation across all difficulties.
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, true);
            this.Push(new EditorLoader());
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            revertMods();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            revertMods();
            return false;
        }

        private ModAutoplay? getAutoplayMod() => Ruleset.Value.CreateInstance().GetAutoplayMod();

        private void revertMods()
        {
            if (playerLoader == null) return;

            Mods.Value = modsAtGameplayStart;
            playerLoader = null;
        }

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
