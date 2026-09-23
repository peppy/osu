// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    public partial class LegacyRankingDetails : CompositeDrawable
    {
        private OsuSpriteText mainText = null!;
        private OsuSpriteText beatmapAuthorText = null!;
        private OsuSpriteText playerText = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 60 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            InternalChildren =
            [
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                },
                mainText = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 22 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR),
                },
                beatmapAuthorText = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 16 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR),
                    Position = new Vector2(1, 20) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
                playerText = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 16 * LegacySkin.STABLE_MAGIC_SCALE_FACTOR),
                    Position = new Vector2(1, 34) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            Debug.Assert(score.Value.Beatmap != null);

            mainText.Text = new RomanisableString(
                $"{score.Value.Beatmap.Metadata.ArtistUnicode} - {score.Value.Beatmap.Metadata.TitleUnicode} [{score.Value.Beatmap.DifficultyName}]",
                $"{score.Value.Beatmap.Metadata.Artist} - {score.Value.Beatmap.Metadata.Title} [{score.Value.Beatmap.DifficultyName}]"
            );
            beatmapAuthorText.Text = LocalisableString.Interpolate($"Beatmap by {score.Value.Beatmap.Metadata.Author.Username}");
            playerText.Text = LocalisableString.Interpolate($"Played by {score.Value.User.Username} on {score.Value.Date:G}.");
        }
    }
}
