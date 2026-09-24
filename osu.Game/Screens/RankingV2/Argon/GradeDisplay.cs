// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class GradeDisplay : CompositeDrawable
    {
        private Container gradedCirclesContainer = null!;
        private Sprite rankSprite = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            Padding = new MarginPadding
            {
                Right = 20,
            };
            InternalChildren =
            [
                new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 2,
                    Colour = Colour4.Black.Opacity(0.3f),
                },
                new Container
                {
                    Size = new Vector2(400),
                    Margin = new MarginPadding(20),
                    Children =
                    [
                        gradedCirclesContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        rankSprite = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fit,
                        }
                    ]
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            var scoreProcessor = score.Value.Ruleset.CreateInstance().CreateScoreProcessor();

            gradedCirclesContainer.Child = new GradedCirclesV2(scoreProcessor)
            {
                RelativeSizeAxes = Axes.Both,
                Progress = score.Value.Accuracy,
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    // in the design this was an inner glow, but framework can't do that, and this kind of has... more sauce anyway?
                    Type = EdgeEffectType.Glow,
                    Hollow = true,
                    Colour = OsuColour.ForRank(score.Value.Rank),
                    Radius = 50,
                },
            };

            rankSprite.Texture = skinManager.DefaultClassicSkin.GetTexture(DrawableRank.GetLegacyRankTextureName(score.Value.Rank));
        }
    }
}
