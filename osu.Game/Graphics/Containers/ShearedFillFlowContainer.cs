// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public sealed partial class ShearedFillFlowContainer : FillFlowContainer<Container>
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        protected override IEnumerable<Vector2> ComputeLayoutPositions()
        {
            var layout = base.ComputeLayoutPositions();

            float drawHeight = DrawHeight;

            foreach (var c in Children)
            {
                float shearWidth = shear.X * drawHeight;
                float relativeY = drawHeight == 0 ? 0 : c.ToSpaceOfOtherDrawable(Vector2.Zero, this).Y / drawHeight;
                c.Padding = new MarginPadding { Right = shearWidth * relativeY };
            }

            return layout;
        }
    }
}
