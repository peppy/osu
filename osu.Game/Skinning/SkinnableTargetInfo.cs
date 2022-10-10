// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Rulesets;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    [Serializable]
    public class SkinnableTargetInfo
    {
        public IEnumerable<SkinnableInfo> AllComponents => DrawableComponentInfo.Values.SelectMany(i => i);

        [JsonProperty]
        public Dictionary<string?, SkinnableInfo[]> DrawableComponentInfo { get; set; } = new Dictionary<string?, SkinnableInfo[]>();

        public bool TryGetComponents(Ruleset? ruleset, out SkinnableInfo[] components) => DrawableComponentInfo.TryGetValue(ruleset?.ShortName ?? string.Empty, out components);

        public void Reset(Ruleset? ruleset)
        {
            DrawableComponentInfo.Remove(ruleset?.ShortName ?? string.Empty);
        }

        public void Update(Ruleset? ruleset, SkinnableInfo[] components)
        {
            DrawableComponentInfo[ruleset?.ShortName ?? string.Empty] = components;
        }
    }
}
