// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class GradeDisplay : CompositeDrawable
    {
        private Container gradedCirclesContainer = null!;
        private Sprite rankSprite = null!;

        private Drawable glowLayer = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren =
            [
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 2,
                    Masking = true,
                    EdgeEffect = BeatmapInfoWedge.CreateShadowEdgeEffect(),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientHorizontal(
                            colourProvider.Background4.Opacity(0.99f),
                            colourProvider.Background4.Opacity(0.9f)
                        ),
                    },
                },
                new Container
                {
                    Size = new Vector2(600),
                    Margin = new MarginPadding(20),
                    Children =
                    [
                        gradedCirclesContainer = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            EdgeEffect = BeatmapInfoWedge.CreateShadowEdgeEffect()
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
                },
                glowLayer = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Height = 2,
                    Blending = BlendingParameters.Additive,
                },
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
            };

            glowLayer.Colour = ColourInfo.GradientHorizontal(
                OsuColour.ForRank(score.Value.Rank).Opacity(0.0f),
                OsuColour.ForRank(score.Value.Rank).Opacity(0.3f)
            );

            // glowContainer.EdgeEffect = new EdgeEffectParameters
            // {
            //     // in the design this was an inner glow, but framework can't do that, and this kind of has... more sauce anyway?
            //     Type = EdgeEffectType.Glow,
            //     // adjust opacity depending on rank type maybe? or just add more flair in a different way.
            //     Colour = OsuColour.ForRank(score.Value.Rank).Opacity(0.05f),
            //     Radius = 50,
            // };

            rankSprite.Texture = skinManager.DefaultClassicSkin.GetTexture(DrawableRank.GetLegacyRankTextureName(score.Value.Rank));
        }
    }
}
