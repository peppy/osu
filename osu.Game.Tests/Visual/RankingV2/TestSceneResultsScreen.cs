// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Argon;
using osu.Game.Screens.RankingV2.Legacy;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Ranking;
using Realms;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneResultsScreen : ScreenTestScene
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private int onlineScoreID = 1;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realm.Run(r =>
            {
                var beatmapInfo = r.All<BeatmapInfo>()
                                   .Filter($"{nameof(BeatmapInfo.Ruleset)}.{nameof(RulesetInfo.OnlineID)} = $0 AND {nameof(BeatmapInfo.StatusInt)} = 1", 0)
                                   .FirstOrDefault();

                if (beatmapInfo != null)
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);
            });
        }

        [Test]
        public void TestArgonScreen()
        {
            AddStep("create argon screen", () =>
            {
                var score = createTestScore();
                LoadScreen(new ArgonResultsScreenV2(score));
            });
        }

        [Test]
        public void TestLegacyScreen()
        {
            AddStep("set legacy skin", () =>
            {
                skins.CurrentSkinInfo.Value = skins.DefaultClassicSkin.SkinInfo;
            });
            AddStep("create legacy screen", () =>
            {
                var score = createTestScore();
                LoadScreen(new LegacyResultsScreenV2(score));
            });
        }

        private IScoreInfo createTestScore()
        {
            var score = TestResources.CreateTestScoreInfo(new OsuRuleset().RulesetInfo);

            score.OnlineID = onlineScoreID++;
            score.HitEvents = TestSceneStatisticsPanel.CreatePositionDistributedHitEvents();
            score.Accuracy = 0.92;
            score.Rank = ScoreRank.A;
            score.PP = 138.4234;

            score.Statistics[HitResult.Miss] = 2;

            score.BeatmapInfo = Beatmap.Value.BeatmapInfo;

            return score;
        }
    }
}
