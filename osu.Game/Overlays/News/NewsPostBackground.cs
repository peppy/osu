// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.News
{
    [LongRunningLoad]
    public partial class NewsPostBackground : Sprite
    {
        private readonly string sourceUrl;

        public NewsPostBackground(string sourceUrl)
        {
            this.sourceUrl = sourceUrl;
        }

        [BackgroundDependencyLoader]
        private async Task load(LargeTextureStore textures, CancellationToken cancellationToken)
        {
            Texture = await textures.GetAsync(createUrl(sourceUrl), cancellationToken).ConfigureAwait(true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(500, Easing.OutQuint);
        }

        private string createUrl(string source)
        {
            if (string.IsNullOrEmpty(source))
                return "Headers/news";

            if (source.StartsWith('/'))
                return "https://osu.ppy.sh" + source;

            return source;
        }
    }
}
