// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Editor
{
    internal class TextureReplacementToolbox : EditorSidebarSection
    {
        protected override Container<Drawable> Content { get; }

        [Resolved]
        private SkinManager skins { get; set; }

        public TextureReplacementToolbox(Drawable component)
            : base($"Textures ({component.GetType().Name})")
        {
            base.Content.Add(Content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
            });
        }

        protected override void Update()
        {
            base.Update();

            var dropAreas = Content.Children.OfType<TextureDropArea>();

            foreach (string lookup in skins.CurrentSkin.Value.TextureLookupsToDate)
            {
                if (dropAreas.All(da => da.AssetName != lookup))
                    Add(new TextureDropArea(lookup));
            }
        }

        public class TextureDropArea : ToolboxButton, ICanAcceptFiles
        {
            public readonly string AssetName;

            [Resolved(canBeNull: true)]
            private OsuGame game { get; set; }

            [Resolved]
            private SkinEditorOverlay skinEditor { get; set; }

            [Resolved]
            private SkinManager skins { get; set; }

            public TextureDropArea(string assetName)
            {
                AssetName = assetName;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AddRange(new Drawable[]
                {
                    new SkinnableSprite(AssetName, ConfineMode.ScaleToFit),
                    new Box
                    {
                        Colour = colourProvider.Background2,
                        RelativeSizeAxes = Axes.X,
                        Height = OsuFont.DEFAULT_FONT_SIZE,
                        Alpha = 0.8f,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                    },
                    new OsuSpriteText
                    {
                        Text = AssetName,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                    },
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                game?.RegisterImportHandler(this);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                game?.UnregisterImportHandler(this);
            }

            protected override bool OnHover(HoverEvent e) => true;

            public Task Import(params string[] paths)
            {
                if (!IsHovered)
                    return Task.CompletedTask;

                Schedule(() =>
                {
                    Background.FlashColour(Color4.SkyBlue, 500);

                    var file = new FileInfo(paths.First());

                    // import to skin
                    skins.CurrentSkin.Value.SkinInfo.PerformWrite(skinInfo =>
                    {
                        using (var contents = file.OpenRead())
                            skins.AddFile(skinInfo, contents, AssetName);
                    });

                    // save is required for now, as we are triggering a global skin refresh,
                    // which will nuke any ongoing changes.
                    skinEditor.Save();
                    skins.TriggerSkinChanged();
                });
                return Task.CompletedTask;
            }

            public Task Import(params ImportTask[] tasks) => throw new NotImplementedException();

            public IEnumerable<string> HandledExtensions => new[] { ".jpg", ".jpeg", ".png" };
        }
    }
}
