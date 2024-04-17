// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    public partial class PredefinedSkinComponentsContainer : SkinComponentsContainer
    {
        private readonly Func<Container> content;

        private SkinLayoutInfo? layoutInfo;

        public PredefinedSkinComponentsContainer(PredefinedSkinComponentsContainerLookup lookup, Func<Container> content)
            : base(lookup)
        {
            this.content = content;

            Components.BindCollectionChanged(componentsChanged);
        }

        private void componentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ISerialisableDrawable d in e.NewItems!)
                    {
                        var info = layoutInfo?.AllDrawables.FirstOrDefault(di => d.GetType() == di.Type);
                        if (info != null)
                            ((Drawable)d).ApplySerialisedInfo(info);
                    }

                    break;
            }
        }

        public override void Reload()
        {
            layoutInfo = CurrentSkin.GetLayoutInfo(Lookup);

            if (layoutInfo == null)
                return;

            Reload(content());
        }
    }
}
