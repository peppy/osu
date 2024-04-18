// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// TODO: This currently exists only to show the components in the skin editor.
    /// It could not exist here and be moved local to the skin editor (likely more appealing).
    /// </summary>
    public partial class ConfigurationLayerExposer : SkinReloadableDrawable, ISerialisableDrawableContainer
    {
        public SkinComponentsContainerLookup Lookup { get; }

        public IBindableList<ISerialisableDrawable> Components => components;

        private readonly BindableList<ISerialisableDrawable> components = new BindableList<ISerialisableDrawable>();

        public ConfigurationLayerExposer(DrawableRuleset target)
        {
            Lookup = new SkinComponentsContainerLookup(SkinComponentsContainerLookup.TargetArea.Configuration, target.Ruleset.RulesetInfo);

            foreach (var d in target.ChildrenOfType<ISerialisableDrawable>())
                components.Add(d);
        }

        public bool ComponentsLoaded => true;
        public bool AcceptsUserDrawables => false;

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
