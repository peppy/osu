// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract class SkinnableTestScene : OsuGridTestScene, IStorageResourceProvider
    {
        private Skin defaultArgon;

        [Resolved]
        private GameHost host { get; set; }

        protected SkinnableTestScene()
            : base(1, 1)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            defaultArgon = new DefaultSkinArgon(this);
        }

        private readonly List<Drawable> createdDrawables = new List<Drawable>();

        protected void SetContents(Func<ISkin, Drawable> creationFunction)
        {
            createdDrawables.Clear();

            var beatmap = CreateBeatmapForSkinProvider();

            Cell(0).Child = createProvider(defaultArgon, creationFunction, beatmap);
        }

        protected IEnumerable<Drawable> CreatedDrawables => createdDrawables;

        private Drawable createProvider(Skin skin, Func<ISkin, Drawable> creationFunction, IBeatmap beatmap)
        {
            var created = creationFunction(skin);

            createdDrawables.Add(created);

            Container childContainer;
            OutlineBox outlineBox;
            SkinProvidingContainer skinProvider;

            ISkin provider = Ruleset.Value.CreateInstance().CreateSkinTransformer(skin, beatmap) ?? skin;

            var children = new Container
            {
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.White,
                BorderThickness = 5,
                Masking = true,

                Children = new Drawable[]
                {
                    new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Text = skin?.SkinInfo.Value.Name ?? "none",
                        Scale = new Vector2(1.5f),
                        Padding = new MarginPadding(5),
                    },
                    childContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            outlineBox = new OutlineBox(),
                            skinProvider = new SkinProvidingContainer(provider)
                            {
                                Child = created,
                            }
                        }
                    },
                }
            };

            // run this once initially to bring things into a sane state as early as possible.
            updateSizing();

            // run this once after construction to handle the case the changes are made in a BDL/LoadComplete call.
            Schedule(updateSizing);

            return children;

            void updateSizing()
            {
                bool autoSize = created.RelativeSizeAxes == Axes.None;

                foreach (var c in new[] { childContainer, skinProvider })
                {
                    c.RelativeSizeAxes = Axes.None;
                    c.AutoSizeAxes = Axes.None;

                    c.RelativeSizeAxes = !autoSize ? Axes.Both : Axes.None;
                    c.AutoSizeAxes = autoSize ? Axes.Both : Axes.None;
                }

                outlineBox.Alpha = autoSize ? 1 : 0;
            }
        }

        /// <summary>
        /// Creates the ruleset for adding the corresponding skin transforming component.
        /// </summary>
        [NotNull]
        protected abstract Ruleset CreateRulesetForSkinProvider();

        protected sealed override Ruleset CreateRuleset() => CreateRulesetForSkinProvider();

        protected virtual IBeatmap CreateBeatmapForSkinProvider() => CreateWorkingBeatmap(Ruleset.Value).GetPlayableBeatmap(Ruleset.Value);

        #region IResourceStorageProvider

        public IRenderer Renderer => host.Renderer;
        public AudioManager AudioManager => Audio;
        public IResourceStore<byte[]> Files => null;
        public new IResourceStore<byte[]> Resources => base.Resources;
        public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);
        RealmAccess IStorageResourceProvider.RealmAccess => null;

        #endregion

        private class OutlineBox : CompositeDrawable
        {
            public OutlineBox()
            {
                BorderColour = Color4.IndianRed;
                BorderThickness = 5;
                Masking = true;
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = Color4.Brown,
                    AlwaysPresent = true
                };
            }
        }
    }
}
