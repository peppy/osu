// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    // TODO: tooltip with extended info
    public partial class LegacyRankingGraph : CompositeDrawable
    {
        private Sprite perfectIndicator = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;

            // TODO: move to skinnable container defaults
            bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;
            var baselinePosition = new Vector2(160, useNewLayout ? 380 : 360);
            Position = baselinePosition * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            var graphSize = new Vector2(186, 86) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;
            const float graph_path_radius = 3;
            var pathSize = graphSize - new Vector2(graph_path_radius);

            SmoothPath failingPath;

            // TODO: data is completely mocked because there's no way to get it even - make it work properly
            IReadOnlyList<Vector2> hpGraphVertices =
            [
                new Vector2(0f, 0f) * pathSize,
                new Vector2(0.2f, 0.3f) * pathSize,
                new Vector2(0.4f, 0.2f) * pathSize,
                new Vector2(0.6f, 0.8f) * pathSize,
                new Vector2(0.7f, 0.4f) * pathSize,
                new Vector2(0.9f, 0.7f) * pathSize,
                new Vector2(1f, 0.45f) * pathSize,
            ];
            InternalChildren =
            [
                new Sprite
                {
                    Texture = skin.GetTexture(@"ranking-graph"),
                },
                new Container
                {
                    Position = new Vector2(useNewLayout ? 5 : 10) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Size = graphSize + new Vector2(graph_path_radius, 0),
                    Children =
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Height = 0.5f,
                            Colour = Colour4.YellowGreen,
                            Child = new SmoothPath
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                PathRadius = graph_path_radius,
                                Vertices = hpGraphVertices,
                            },
                        },
                        new Container
                        {
                            RelativePositionAxes = Axes.Y,
                            Y = 0.5f,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Height = 0.5f,
                            Colour = Colour4.Red,
                            Child = failingPath = new SmoothPath
                            {
                                Anchor = Anchor.BottomLeft,
                                PathRadius = graph_path_radius,
                                Vertices = hpGraphVertices,
                            },
                        },
                    ],
                },
                perfectIndicator = new Sprite
                {
                    Texture = skin.GetTexture(@"ranking-perfect"),
                    Position = (new Vector2(useNewLayout ? 260 : 200, 430) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Origin = Anchor.Centre,
                },
            ];

            failingPath.OriginPosition = failingPath.PositionInBoundingBox(new Vector2(-graph_path_radius, -graph_path_radius + graphSize.Y));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            perfectIndicator.Alpha = score.Value.MaxCombo == score.Value.GetMaximumAchievableCombo() ? 1 : 0;
        }
    }
}
