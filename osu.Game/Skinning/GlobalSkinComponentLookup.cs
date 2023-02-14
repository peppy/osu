// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public class GlobalSkinComponentLookup : ISkinComponentLookup
    {
        public readonly LookupType Lookup;

        public readonly Ruleset? Ruleset;

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
}
