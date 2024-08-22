// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Extensions;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Represents a lookup of a collection of elements that make up a particular skinnable <see cref="GlobalSkinnableContainers"/> of the game.
    /// </summary>
    public class GlobalSkinnableContainerLookup : SkinComponentLookup<GlobalSkinnableContainerLookup.GlobalSkinnableContainers>, IEquatable<GlobalSkinnableContainerLookup>
    {
        public readonly RulesetInfo? Ruleset;

        // TODO: remove maybe
        public GlobalSkinnableContainers Target => Component;

        public GlobalSkinnableContainerLookup(GlobalSkinnableContainers component, RulesetInfo? ruleset = null)
            : base(component)
        {
            Ruleset = ruleset;
        }

        public override string ToString()
        {
            if (Ruleset == null) return Target.GetDescription();

            return $"{Target.GetDescription()} (\"{Ruleset.Name}\" only)";
        }

        public bool Equals(GlobalSkinnableContainerLookup? other)
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

            return Equals((GlobalSkinnableContainerLookup)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Target, Ruleset);
        }

        /// <summary>
        /// Represents a particular area or part of a game screen whose layout can be customised using the skin editor.
        /// </summary>
        public enum GlobalSkinnableContainers
        {
            [Description("HUD")]
            MainHUDComponents,

            [Description("Song select")]
            SongSelect,

            [Description("Playfield")]
            Playfield
        }
    }
}
