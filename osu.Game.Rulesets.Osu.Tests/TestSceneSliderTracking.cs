// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSliderTracking : OsuSkinnableTestScene
    {
        private const double time_offset = 1500;

        private const float max_length = 200;

        private int depthIndex;

        [Test]
        public void TestTracking()
        {
            AddStep("Big Single", () => SetContents(_ =>
            {
                var simpleBig = createSlider();
                simpleBig.Tracking.Value = true;
                return simpleBig;
            }));
        }

        private DrawableSlider createSlider(float circleSize = 2, float distance = max_length, int repeats = 0, double speedMultiplier = 2, int stackHeight = 0)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + time_offset,
                Position = new Vector2(0, -(distance / 2)),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(0, distance),
                }, distance),
                RepeatCount = repeats,
                StackHeight = stackHeight
            };

            return createDrawable(slider, circleSize, speedMultiplier);
        }

        private DrawableSlider createDrawable(Slider slider, float circleSize, double speedMultiplier)
        {
            var cpi = new LegacyControlPointInfo();
            cpi.Add(0, new DifficultyControlPoint { SliderVelocity = speedMultiplier });

            slider.ApplyDefaults(cpi, new BeatmapDifficulty
            {
                CircleSize = circleSize,
                SliderTickRate = 3
            });

            var drawable = CreateDrawableSlider(slider);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawable);

            return drawable;
        }

        protected virtual DrawableSlider CreateDrawableSlider(Slider slider) => new DrawableSlider(slider)
        {
            Anchor = Anchor.Centre,
            Depth = depthIndex++
        };
    }
}
