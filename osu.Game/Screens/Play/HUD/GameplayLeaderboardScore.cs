// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class GameplayLeaderboardScore : CompositeDrawable, ILeaderboardScore
    {
        public BindableLong TotalScore { get; } = new BindableLong();
        public BindableDouble Accuracy { get; } = new BindableDouble(1);
        public BindableInt Combo { get; } = new BindableInt();
        public BindableBool HasQuit { get; } = new BindableBool();
        public Bindable<long> DisplayOrder { get; } = new Bindable<long>();

        public Func<ScoringMode, long>? GetDisplayScore { protected get; set; }

        public Color4? BackgroundColour { get; set; }
        public Color4? TextColour { get; set; }

        public readonly Bindable<bool> Expanded = new Bindable<bool>();

        /// <summary>
        /// Whether this score is the local user or a replay player (and should be focused / always visible).
        /// </summary>
        public readonly bool Tracked;

        public virtual int? ScorePosition { get; set; }

        public IUser? User { get; }

        private IBindable<ScoringMode> scoreDisplayMode = null!;

        /// <summary>
        /// Creates a new <see cref="GameplayLeaderboardScore"/>.
        /// </summary>
        /// <param name="user">The score's player.</param>
        /// <param name="tracked">Whether the player is the local user or a replay player.</param>
        protected GameplayLeaderboardScore(IUser? user, bool tracked)
        {
            User = user;
            Tracked = tracked;

            GetDisplayScore = _ => TotalScore.Value;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager osuConfigManager)
        {
            scoreDisplayMode = osuConfigManager.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreDisplayMode.BindValueChanged(_ => updateScore());
            TotalScore.BindValueChanged(_ => updateScore(), true);

            void updateScore() => UpdateScore((GetDisplayScore?.Invoke(scoreDisplayMode.Value) ?? TotalScore.Value).ToString(scoreDisplayMode.Value == ScoringMode.Classic ? "00000000" : "000000"));
        }

        protected abstract void UpdateScore(string scoreString);
    }
}
