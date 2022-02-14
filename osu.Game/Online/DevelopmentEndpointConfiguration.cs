// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class DevelopmentEndpointConfiguration : EndpointConfiguration
    {
        public DevelopmentEndpointConfiguration()
        {
            WebsiteRootUrl = APIEndpointUrl = @"http://localhost:8080";
            APIClientSecret = @"QQQrBwIoIjxBzVaZxuIVP47qm4QMGubYAQBru504";
            APIClientID = "1";
            SpectatorEndpointUrl = "http://localhost:80/spectator";
            MultiplayerEndpointUrl = "http://localhost:80/multiplayer";
        }
    }
}
