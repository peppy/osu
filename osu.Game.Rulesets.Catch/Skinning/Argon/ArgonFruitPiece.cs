// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    internal class ArgonFruitPiece : CatchHitObjectPiece
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected override HyperBorderPiece HyperBorderPiece { get; }

        public ArgonFruitPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                // new SmoothPath
                // {
                //     Anchor = Anchor.Centre,
                //     Origin = Anchor.Centre,
                //     Alpha = 0.15f,
                //     PathRadius = 22.5f,
                //     Vertices = generateRandomCircle(),
                // },
                // new SmoothPath
                // {
                //     Anchor = Anchor.Centre,
                //     Origin = Anchor.Centre,
                //     Alpha = 0.5f,
                //     PathRadius = 9,
                //     Vertices = generateRandomCircle(),
                // },
                new SmoothPath
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    PathRadius = 3,
                    Vertices = generateRandomCircle(),
                },
                HyperBorderPiece = new HyperBorderPiece()
            };
        }

        private static List<Vector2> generateRandomCircle()
        {
            List<Vector2> vertices = new List<Vector2>();
            List<float> variances = new List<float>();

            const float variance_per_point = 0.8f;
            const int point_count = 8;
            const float radius = 64;

            float variance = 0;

            for (int i = 0; i <= point_count; i++)
            {
                float progress = (float)i / point_count;

                switch (i)
                {
                    case 0:
                        variance = RNG.NextSingle(1 - variance_per_point, 1 + variance_per_point);
                        break;

                    default:
                        variance += RNG.NextSingle(
                            -variance_per_point * (float)Interpolation.ApplyEasing(Easing.OutQuint, MathHelper.Clamp(variance - 1, 0, 1)),
                            variance_per_point * (float)Interpolation.ApplyEasing(Easing.OutQuint, MathHelper.Clamp(1 - variance, 0, 1))
                        );
                        break;
                }

                variances.Add(variance);

                vertices.Add(
                    radius * variance * new Vector2(
                        MathF.Sin(progress * MathF.PI * 2),
                        MathF.Cos(progress * MathF.PI * 2)
                    ));
            }

            var catmullPart = PathApproximator.ApproximateCatmull(vertices.Take(vertices.Count - 1).ToArray()).Skip(1);



            // to connect the start to end, let's use a bezier with the correct derivative variance at each end.
            var bezierPart = PathApproximator.ApproximateBezier(vertices.TakeLast(2).Concat(vertices.Take(2)).ToArray());

            return catmullPart
                   //.Concat(bezierPart.Skip(1).SkipLast(1))
                   .ToList();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IndexInBeatmap.BindValueChanged(index =>
            {
                VisualRepresentation.Value = Fruit.GetVisualRepresentation(index.NewValue);
            }, true);

            AccentColour.BindValueChanged(colour => InternalChildren.OfType<SmoothPath>().ForEach(s => s.Colour = colour.NewValue), true);
        }
    }
}
