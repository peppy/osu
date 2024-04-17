// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public partial class SkinConfigurableDrawable : SkinReloadableDrawable
    {
        private readonly Action<Drawable> applyDefaults;

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
        public SkinConfigurableDrawable(ISerialisableDrawable drawable, Action<Drawable> applyDefaults)
        {
            this.applyDefaults = applyDefaults;

            RelativeSizeAxes = Axes.Both;
            Drawable = drawable;
            InternalChild = (Drawable)Drawable;
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            applyDefaults((Drawable)Drawable);
            skin.ConfigureComponent(Drawable);
        }
    }
}
