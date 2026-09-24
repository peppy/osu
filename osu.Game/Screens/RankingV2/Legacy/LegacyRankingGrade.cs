// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    public partial class LegacyRankingGrade : CompositeDrawable
    {
        private Sprite gradeSprite = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            // TODO: move to skinnable container defaults
            bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;
            Anchor = Anchor.TopRight;
            Origin = Anchor.Centre;
            Position = new Vector2(-120, useNewLayout ? 200 : 170) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            InternalChild = gradeSprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            gradeSprite.Size = Vector2.Zero;
            gradeSprite.Texture = skin.GetTexture($@"ranking-{score.Value.Rank.ToString()}");
        }
    }
}
