// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes a container which can house <see cref="ISkinnableDrawable"/>s.
    /// </summary>
    public interface ISkinnableTarget : IDrawable
    {
        /// <summary>
        /// The definition of this target.
        /// </summary>
        GlobalSkinComponentLookup.LookupType Target { get; }

        /// <summary>
        /// The ruleset which this target should load components for, or null if there's no specific ruleset for this target.
        /// </summary>
        Ruleset? Ruleset { get; }

        /// <summary>
        /// A bindable list of components which are being tracked by this skinnable target.
        /// </summary>
        IBindableList<ISkinnableDrawable> Components { get; }

        /// <summary>
        /// Serialise non-ruleset-specific children as <see cref="SkinnableDrawableInfo"/>.
        /// </summary>
        /// <returns>The serialised content.</returns>
        IEnumerable<SkinnableDrawableInfo> CreateSkinnableInfo() => Components.Select(d => ((Drawable)d).CreateSkinnableInfo());

        /// <summary>
        /// Reload this target from the current skin.
        /// </summary>
        void Reload();

        /// <summary>
        /// Reload this target from the provided skinnable information.
        /// </summary>
        void Reload(SkinnableDrawableInfo[] skinnableInfo);

        /// <summary>
        /// Add a new skinnable component to this target.
        /// </summary>
        /// <param name="drawable">The component to add.</param>
        void Add(ISkinnableDrawable drawable);

        /// <summary>
        /// Remove an existing skinnable component from this target.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        void Remove(ISkinnableDrawable component);
    }
}
