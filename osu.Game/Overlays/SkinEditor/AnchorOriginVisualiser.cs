// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinEditor
{
    internal partial class AnchorOriginVisualiser : CompositeDrawable
    {
        private readonly Drawable drawable;

        private Drawable originBox = null!;

        private Drawable anchorBox = null!;
        private Drawable anchorLine = null!;

        public AnchorOriginVisualiser(Drawable drawable)
        {
            this.drawable = drawable;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Color4 anchorColour = colours.Red1;
            Color4 originColour = colours.Red3;

            InternalChildren = new[]
            {
                anchorLine = new Circle
                {
                    Height = 3f,
                    Origin = Anchor.CentreLeft,
                    Colour = ColourInfo.GradientHorizontal(originColour.Opacity(0.5f), originColour),
                },
                originBox = new Circle
                {
                    Colour = originColour,
                    Origin = Anchor.Centre,
                    Size = new Vector2(7),
                },
                anchorBox = new Circle
                {
                    Colour = anchorColour,
                    Origin = Anchor.Centre,
                    Size = new Vector2(10),
                },
            };
        }

        private Vector2? anchorPosition;
        private Vector2? originPositionInDrawableSpace;

        protected override void Update()
        {
            base.Update();

            if (drawable.Parent == null)
                return;

            var newAnchor = drawable.Parent!.ToSpaceOfOtherDrawable(drawable.AnchorPosition, this);
            anchorPosition = tweenPosition(anchorPosition ?? newAnchor, newAnchor);
            anchorBox.Position = anchorPosition.Value;

            // for the origin, tween in the drawable's local space to avoid unwanted tweening when the drawable is being dragged.
            originPositionInDrawableSpace = originPositionInDrawableSpace != null ? tweenPosition(originPositionInDrawableSpace.Value, drawable.OriginPosition) : drawable.OriginPosition;
            originBox.Position = drawable.ToSpaceOfOtherDrawable(originPositionInDrawableSpace.Value, this);

            var point1 = ToLocalSpace(anchorBox.ScreenSpaceDrawQuad.Centre);
            var point2 = ToLocalSpace(originBox.ScreenSpaceDrawQuad.Centre);

            anchorLine.Position = point1;
            anchorLine.Width = (point2 - point1).Length;
            anchorLine.Rotation = MathHelper.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X));
        }

        private Vector2 tweenPosition(Vector2 oldPosition, Vector2 newPosition)
            => new Vector2(
                (float)Interpolation.DampContinuously(oldPosition.X, newPosition.X, 25, Clock.ElapsedFrameTime),
                (float)Interpolation.DampContinuously(oldPosition.Y, newPosition.Y, 25, Clock.ElapsedFrameTime)
            );
    }
}
