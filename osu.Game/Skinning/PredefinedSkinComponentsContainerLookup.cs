// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// TODO
    /// </summary>
    public class PredefinedSkinComponentsContainerLookup : ISkinComponentLookup, IEquatable<PredefinedSkinComponentsContainerLookup>
    {
        /// <summary>
        /// The target area / layer of the game for which skin components will be returned.
        /// </summary>
        public readonly string Target;

        /// <summary>
        /// The ruleset for which skin components should be returned.
        /// A <see langword="null"/> value means that returned components are global and should be applied for all rulesets.
        /// </summary>
        public readonly RulesetInfo? Ruleset;

        public PredefinedSkinComponentsContainerLookup(string target, RulesetInfo? ruleset = null)
        {
            Target = target;
            Ruleset = ruleset;
        }

        public bool Equals(PredefinedSkinComponentsContainerLookup? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Target == other.Target && (ReferenceEquals(Ruleset, other.Ruleset) || Ruleset?.Equals(other.Ruleset) == true);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((PredefinedSkinComponentsContainerLookup)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Target, Ruleset);
        }

        bool IEquatable<ISkinComponentLookup>.Equals(ISkinComponentLookup? other)
            => other is PredefinedSkinComponentsContainerLookup lookup && Equals(lookup);

        object ISkinComponentLookup.Target => Target;

        RulesetInfo? ISkinComponentLookup.Ruleset => Ruleset;
    }
}
