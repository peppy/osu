// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Applies skin configuration to all <see cref="IConfigurableDrawable"/>s within a <see cref="DrawableRuleset"/>.
    /// This also exposes the components to the skin editor as a <see cref="ISerialisableDrawableContainer"/>.
    /// </summary>
    public partial class SkinConfigurationApplier : SkinReloadableDrawable, ISerialisableDrawableContainer
    {
        public SkinComponentsContainerLookup Lookup { get; }

        public IBindableList<ISerialisableDrawable> Components => components;

        private readonly BindableList<ISerialisableDrawable> components = new BindableList<ISerialisableDrawable>();

        public SkinConfigurationApplier(DrawableRuleset target)
        {
            Lookup = new SkinComponentsContainerLookup(SkinComponentsContainerLookup.TargetArea.Configuration, target.Ruleset.RulesetInfo);

            if (target.IsLoaded)
                iterateDrawables(target);
            else
                target.OnLoadComplete += iterateDrawables;
        }

        private void iterateDrawables(Drawable target)
        {
            foreach (var d in target.ChildrenOfType<ISerialisableDrawable>())
                components.Add(d);
        }

        public bool ComponentsLoaded => true;
        public bool AcceptsUserDrawables => false;

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            foreach (var c in components)
            {
                (c as IConfigurableDrawable)?.ApplyDefaults();
                skin.ConfigureComponent(c);
            }
        }

        public void Reload()
        {
        }

        public void Reload(SerialisedDrawableInfo[] skinnableInfo)
        {
        }

        public void Add(ISerialisableDrawable drawable)
        {
        }

        public void Remove(ISerialisableDrawable component, bool disposeImmediately)
        {
        }
    }
}
