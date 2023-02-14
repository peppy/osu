// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public class GlobalSkinComponentLookup : ISkinComponent
    {
        public readonly LookupType Lookup;
        public readonly Ruleset? Ruleset;

        public string LookupName => Ruleset == null ? $"{Lookup}" : $"{Lookup}-{Ruleset.ShortName}";

        public GlobalSkinComponentLookup(LookupType lookup, Ruleset? ruleset = null)
        {
            Lookup = lookup;
            Ruleset = ruleset;
        }

        public enum LookupType
        {
            MainHUDComponents,
            RulesetHUDComponents,
            SongSelect
        }
    }

    public static class SkinnableLookupTypeExtensions
    {
        public static bool RequiresRuleset(this GlobalSkinComponentLookup.LookupType target) => target == GlobalSkinComponentLookup.LookupType.RulesetHUDComponents;
    }
}
