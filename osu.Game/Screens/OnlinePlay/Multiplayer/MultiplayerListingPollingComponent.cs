// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerListingPollingComponent : ListingPollingComponent
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly IBindable<bool> isConnected = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load()
        {
            isConnected.BindTo(client.IsConnected);
            isConnected.BindValueChanged(_ => Scheduler.AddOnce(poll), true);
        }

        private void poll()
        {
            if (isConnected.Value && IsLoaded)
                PollImmediately();
        }

        protected override Task Poll()
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            if (client.Room != null)
                return Task.CompletedTask;

            return base.Poll();
        }
    }
}
