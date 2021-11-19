// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Testing;
using Realms;

#nullable enable

namespace osu.Game.Rulesets
{
    [ExcludeFromDynamicCompile]
    [MapTo("Ruleset")]
    public class RulesetInfo : RealmObject, IEquatable<RulesetInfo>, IRulesetInfo
    {
        [PrimaryKey]
        public string ShortName { get; set; } = string.Empty;

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public string Name { get; set; } = string.Empty;

        public string InstantiationInfo { get; set; } = string.Empty;

        public RulesetInfo(string shortName, string name, string instantiationInfo, int? onlineID = null)
        {
            ShortName = shortName;
            Name = name;
            InstantiationInfo = instantiationInfo;
            OnlineID = onlineID ?? -1;
        }

        [UsedImplicitly]
        public RulesetInfo()
        {
        }

        public RulesetInfo(int? onlineID, string name, string shortName, bool available)
        {
            OnlineID = onlineID ?? -1;
            Name = name;
            ShortName = shortName;
            Available = available;
        }

        public bool Available { get; set; }

        public bool Equals(RulesetInfo? other) => other != null && OnlineID == other.OnlineID && Available == other.Available && Name == other.Name && InstantiationInfo == other.InstantiationInfo;

        public override string ToString() => Name;

        public RulesetInfo Clone() => new RulesetInfo
        {
            OnlineID = OnlineID,
            Name = Name,
            ShortName = ShortName,
            InstantiationInfo = InstantiationInfo,
            Available = Available
        };

        #region Compatibility properties

        public int ID => OnlineID;

        public Ruleset? CreateInstance() => ((IRulesetInfo)this).CreateInstance();

        #endregion
    }
}
