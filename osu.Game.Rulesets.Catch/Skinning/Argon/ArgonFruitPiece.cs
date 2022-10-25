// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Catch.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    internal class ArgonFruitPiece : CatchHitObjectPiece
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        private readonly Container buffered;

        protected override Drawable HyperBorderPiece { get; }

        public ArgonFruitPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                buffered = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new SmoothPath
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.15f,
                            PathRadius = 22.5f,
                            Vertices = generateRandomCircle(),
                        },
                        new SmoothPath
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.5f,
                            PathRadius = 9,
                            Vertices = generateRandomCircle(),
                        },
                        new SmoothPath
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Blending = BlendingParameters.Additive,
                            PathRadius = 3,
                            Vertices = generateRandomCircle(),
                        },
                    }
                },
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
                },
                HyperBorderPiece = new SmoothPath
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.15f,
                    PathRadius = 22.5f,
                    Vertices = generateRandomCircle(),
                },
            };
        }

        private static List<Vector2> generateRandomCircle()
        {
            List<Vector2> vertices = new List<Vector2>();
            List<float> variances = new List<float>();

            const float variance_per_point = 0.7f;
            const int point_count = 8;
            const float radius = 40;

            float variance = 0;

            for (int i = 0; i <= point_count; i++)
            {
                float progress = (float)i / point_count;

                switch (i)
                {
                    case 0:
                        variance = RNG.NextSingle(1 - variance_per_point, 1);
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

            const float extension_length = 0.5f;

            // create a connecting bezier using the first and last points of the original curve, extending them and adding a midpoint.
            Vector2 p1 = vertices[^2];
            Vector2 p2 = vertices[^2] + Vector2.Normalize(vertices[^2] - vertices[^3]) * Vector2.Distance(vertices[^3], vertices[^2]) * extension_length;

            Vector2 p3 = vertices[^2] + (vertices[0] - vertices[^2]) / 2;

            Vector2 p4 = vertices[0] - Vector2.Normalize(vertices[1] - vertices[0]) * Vector2.Distance(vertices[0], vertices[1]) * extension_length;
            Vector2 p5 = vertices[0];

            // to connect the start to end, let's use a bezier with the correct derivative variance at each end.
            var bezierPart1 = PathApproximator.ApproximateBezier(new[] { p1, p2, p3, p4, p5 });

            return catmullPart
                   .Concat(bezierPart1)
                   .ToList();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IndexInBeatmap.BindValueChanged(index =>
            {
                VisualRepresentation.Value = Fruit.GetVisualRepresentation(index.NewValue);
            }, true);

            AccentColour.BindValueChanged(colour => buffered.OfType<SmoothPath>().ForEach(s => s.Colour = colour.NewValue), true);
        }
    }
}
