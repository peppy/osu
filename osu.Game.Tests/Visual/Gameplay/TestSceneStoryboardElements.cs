// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneStoryboardElements : OsuTestScene
    {
        private DrawableStoryboard drawableStoryboard = null!;

        private readonly TestStoryboard storyboard = new TestStoryboard();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create storyboard", () =>
            {
                storyboard.BeatmapInfo = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;

                var background = storyboard.GetLayer("Background");

                background.Elements.Clear();

                var sprite = new StoryboardSprite("Resources/Textures/test-image.png", Anchor.Centre, new Vector2(320, 240));
                sprite.AddLoop(Time.Current, 100).Alpha.Add(Easing.None, 0, 10000, 1, 1);
                background.Add(sprite);

                var video = new StoryboardVideo("Resources/Videos/test-video.mp4", Time.Current);
                storyboard.GetLayer("Video").Add(video);

                Child = drawableStoryboard = storyboard.CreateDrawable();
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("Change origin to centre", () =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements)
                        d.Origin = Anchor.Centre;
                }
            });

            AddStep("Change origin to bottom right", () =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements)
                        d.Origin = Anchor.BottomRight;
                }
            });

            AddStep("Change origin to top left", () =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements)
                        d.Origin = Anchor.TopLeft;
                }
            });
        }

        private partial class TestStoryboard : Storyboard
        {
            public override DrawableStoryboard CreateDrawable(IReadOnlyList<Mod>? mods = null)
            {
                return new TestDrawableStoryboard(this, mods);
            }

            public override string? GetStoragePathFromStoryboardPath(string path) => path;

            private partial class TestDrawableStoryboard : DrawableStoryboard
            {
                public TestDrawableStoryboard(Storyboard storyboard, IReadOnlyList<Mod>? mods)
                    : base(storyboard, mods)
                {
                }

                protected override IResourceStore<byte[]> CreateResourceLookupStore() => TestResources.GetStore();
            }
        }
    }
}
