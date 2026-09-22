// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.RankingV2
{
    /*
     * TODO:
     * - Skinnability is probably not going to work via substituting screen implementations.
     *   That is temporary, this is being done via full screens for now just to get a first pass at the layout.
     */
    public partial class ArgonResultsScreenV2 : ScreenWithBeatmapBackground
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue); // to match web score pages

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        // TODO: multiplayer screens accept null score when showing a playlist item's scores - decide how to handle that
        public ArgonResultsScreenV2(IScoreInfo initialScore)
        {
            score.Value = initialScore;
        }

        public override bool ShowFooter => true;

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() =>
        [
            // TODO: replace fontawesome, localise strings, actually hook up actions
            new ScreenFooterButton
            {
                Icon = FontAwesome.Solid.ChartBar,
                Text = "Ranking",
                Action = () => { },
                AccentColour = colours.Green1, // to match web ranking pages
            },
            new ScreenFooterButton
            {
                Icon = FontAwesome.Solid.Search,
                Text = "More statistics",
                Action = () => { },
                AccentColour = colours.Blue1,
            },
            new ScreenFooterButton
            {
                Icon = FontAwesome.Solid.Inbox,
                Text = "Catalogue",
                Action = () => { },
                AccentColour = colours.Blue1, // to match beatmap pages
            },
        ]; // TODO: how to do main buttons on the right?
    }
}
