// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Skinning;

namespace osu.Game.Screens.RankingV2.Legacy
{
    public abstract partial class LegacyRankingButton : CompositeDrawable
    {
        public Action? Action { get; set; }

        private readonly string[] spriteNames;

        private Sprite buttonSprite = null!;

        protected LegacyRankingButton(params string[] spriteNames)
        {
            this.spriteNames = spriteNames;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = buttonSprite = new Sprite();

            foreach (string spriteName in spriteNames)
                buttonSprite.Texture ??= skin.GetTexture(spriteName);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }

        private void updateState()
        {
            buttonSprite.FadeTo(IsHovered ? 1f : 0.7f, 200);
        }
    }

    public partial class LegacyRankingRetryButton : LegacyRankingButton
    {
        public LegacyRankingRetryButton()
            : base(@"ranking-retry", @"pause-retry")
        {
        }
    }

    public partial class LegacyRankingWatchReplayButton : LegacyRankingButton
    {
        public LegacyRankingWatchReplayButton()
            : base(@"ranking-replay", @"pause-replay")
        {
        }
    }
}
