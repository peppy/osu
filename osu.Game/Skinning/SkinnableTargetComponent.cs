// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public class SkinnableTargetComponent : ISkinComponent
    {
        public readonly SkinnableTarget Target;
        public readonly Ruleset? Ruleset;

        public string LookupName => Ruleset == null ? $"{Target}" : $"{Target}-{Ruleset.ShortName}";

        public SkinnableTargetComponent(SkinnableTarget target, Ruleset? ruleset = null)
        {
            Target = target;
            Ruleset = ruleset;
        }
    }
}
