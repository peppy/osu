// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public partial class SkinConfigurableDrawable : SkinReloadableDrawable
    {
        /// <summary>
        /// The displayed component.
        /// </summary>
        public ISerialisableDrawable Drawable { get; private set; }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        /// <summary>
        /// Create a new skinnable drawable.
        /// </summary>
        public SkinConfigurableDrawable(ISerialisableDrawable drawable)
        {
            RelativeSizeAxes = Axes.Both;
            Drawable = drawable;
            InternalChild = (Drawable)Drawable;
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            skin.ConfigureComponent(Drawable);
        }
    }
}
