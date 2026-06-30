// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Benchmarks
{
    public class BenchmarkCreateInstance : BenchmarkTest
    {
        private RulesetInfo rulesetInfo = null!;

        public override void SetUp()
        {
            base.SetUp();
            rulesetInfo = new OsuRuleset().RulesetInfo;
        }

        [Benchmark]
        public Ruleset ModHashCode()
        {
            return rulesetInfo.CreateInstance();
        }
    }
}
