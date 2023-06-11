// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Database
{
    public class ThisIsAnAwesomeClass
    {
        #region Variables

        /// <summary>
        /// The default value for <see cref="field"/>.
        /// Yes, the variable intentionally has a typo.
        /// </summary>
        private const bool field_default_vlaue = true;

        public bool UnusedVariable;

        private readonly bool field;

        #endregion

        public ThisIsAnAwesomeClass()
        {
            // TODO: figure out what this thing is actually supposed to do.
            // although maybe we don't need to, as this whole class only exists for the purpose
            // of a screenshot for https://github.com/peppy/ppy-jetbrains-theme...
            field = field_default_vlaue;


            // warnings here because we should delete this excess whitespace.

            an actual error

            throw new InvalidOperationException($@"This class ({nameof(ThisIsAnAwesomeClass)} is not supposed to actually be used.");
        }

        public bool GetFieldValue() => field;
    }
}
