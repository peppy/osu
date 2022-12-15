// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    public abstract partial class JudgementPiece : CompositeDrawable
    {
        protected readonly HitResult Result;

        protected SpriteText JudgementText { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        private Bindable<bool> displayPerfectAsMax;

        protected JudgementPiece(HitResult result)
        {
            Result = result;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayPerfectAsMax = config.GetBindable<bool>(OsuSetting.DisplayPerfectAsMax);
            displayPerfectAsMax.BindValueChanged(val => JudgementText.Text = Result.GetDisplayString(val.NewValue ? "Max" : "Perfect").ToUpperInvariant(), true);
        }
    }
}
