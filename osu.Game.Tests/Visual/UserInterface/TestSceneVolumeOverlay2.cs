// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneVolumeOverlay2 : OsuTestScene
    {
        private readonly VolumeOverlay overlay;

        public TestSceneVolumeOverlay2()
        {
            Child = overlay = new VolumeOverlay
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("show", () => overlay.Show());
            AddStep("hide", () => overlay.Hide());
        }

        private partial class VolumeOverlay : VisibilityContainer
        {
            private Drawable mainCircle = null!;
            private readonly List<Drawable> rotationContainers = new List<Drawable>();

            private const int num_circles = 3;

            [BackgroundDependencyLoader]
            private void load()
            {
                Padding = new MarginPadding(120);

                for (int i = 0; i < num_circles; i++)
                {
                    var container = new Container
                    {
                        Rotation = angleFor(i),
                        Width = 190,
                        Anchor = Anchor.BottomLeft,
                        Child = new Circle
                        {
                            Size = new Vector2(140),
                            Origin = Anchor.Centre,
                            Anchor = Anchor.CentreRight,
                        },
                    };

                    rotationContainers.Add(container);

                    AddInternal(container);
                }

                AddInternal(mainCircle = new Circle
                {
                    Size = new Vector2(200),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomLeft,
                });
            }

            // -100 -> angled upwards titled slightly left
            // 10 -> angled right titled slightly downwards
            private float angleFor(int index) => float.Lerp(-100, 10, index / (float)(num_circles - 1));

            protected override void PopIn()
            {
                mainCircle.ScaleTo(1f, 600, Easing.OutElasticQuarter)
                          .FadeIn(300);

                for (int i = 0; i < num_circles; i++)
                {
                    rotationContainers[i]
                        .Delay(i * 50)
                        .RotateTo(angleFor(i), 600, Easing.OutElasticQuarter)
                        .ScaleTo(1f, 600, Easing.OutElasticQuarter)
                        .FadeIn(300);
                }

                this.TransformTo(nameof(Padding), new MarginPadding(120), 600, Easing.OutElasticQuarter);
            }

            protected override void PopOut()
            {
                mainCircle.ScaleTo(0.5f, 300, Easing.In)
                          .FadeOut(200);

                for (int i = 0; i < num_circles; i++)
                {
                    rotationContainers[i]
                        .Delay((num_circles - 1 - i) * 30)
                        .RotateTo(angleFor(i) + 90, 200, Easing.In)
                        .ScaleTo(0.5f, 150, Easing.In)
                        .FadeOut(150);
                }

                this.TransformTo(nameof(Padding), new MarginPadding(70), 300, Easing.In);
            }
        }
    }
}
