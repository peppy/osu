// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Models;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// A model that contains a list of files it is responsible for.
    /// </summary>
    public interface IHasRealmFiles
    {
        /// <summary>
        /// A list of all files related to this model.
        /// </summary>
        IList<RealmNamedFileUsage> Files { get; }

        /// <summary>
        /// A unique hash representing this model in its current state.
        /// </summary>
        string Hash { get; set; }
    }
}
