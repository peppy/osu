// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    internal class UserSkinLayoutLookup : ISkinComponentLookup
    {
        public readonly ISkinComponentLookup Component;

        public UserSkinLayoutLookup(ISkinComponentLookup component)
        {
            Component = component;
        }
    }
}
