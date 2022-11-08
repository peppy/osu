// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class RingPiece : CircularContainer, ISkinnableDrawable
    {
        [SettingSource("Border thickness", "How thick")]
        public new BindableNumber<float> BorderThickness { get; } = new BindableNumber<float>(1)
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 1,
        };

        public RingPiece()
            : this(9)
        {
        }

        public RingPiece(float thickness = 9)
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Masking = true;
            BorderColour = Color4.White;

            Child = new Box
            {
                AlwaysPresent = true,
                Alpha = 0,
                RelativeSizeAxes = Axes.Both
            };

            BorderThickness.Default = thickness;
            BorderThickness.Value = thickness;
            BorderThickness.BindValueChanged(borderThickness => base.BorderThickness = borderThickness.NewValue, true);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
