// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    public partial class PredefinedSkinComponentsContainer : SkinComponentsContainer
    {
        private readonly Func<Container> content;

        public PredefinedSkinComponentsContainer(PredefinedSkinComponentsContainerLookup lookup, Func<Container> content)
            : base(lookup)
        {
            this.content = content;
        }

        public override void Reload()
        {
            Reload(content());
            // TODO: apply configuration
        }
    }
}
