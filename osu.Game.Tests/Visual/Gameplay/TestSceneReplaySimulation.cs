// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneReplaySimulation : RateAdjustedBeatmapTestScene
    {
        protected TestReplayPlayer Player = null!;

        [Test]
        public void TestPauseViaSpace()
        {
            var beatmap = new FlatWorkingBeatmap("/Users/dean/Library/Mobile Documents/com~apple~CloudDocs/inabakumori - Lost Umbrella (Ryuusei Aika) [________].osu");

            string[] files = Directory.GetFiles("/Users/dean/Library/Mobile Documents/com~apple~CloudDocs/replay-osu_2992705");

            float[] noiseTests = new[] { 0, 0.1f, 0.5f, 1f, 2f, 5f };

            foreach (float noise in noiseTests)
            {
                LegacyScoreDecoder.Noise = noise;
                OsuHitWindows.UseStableHitWindows = true;

                string filename = $"/Users/Dean/out-{noise}.txt";

                File.Delete(filename);

                foreach (string f in files)
                {
                    LegacyScoreDecoder decoder = new ReplayStabilityTestScene.TestScoreDecoder(beatmap);
                    using var stream = File.OpenRead(f);
                    var score = decoder.Parse(stream);
                    score.ScoreInfo.OnlineID = long.Parse(Path.GetFileNameWithoutExtension(f).Split('_').Last());

                    loadPlayerWithBeatmap(beatmap.Beatmap, score);

                    AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

                    AddStep("seek to half completion", () => Player.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime() / 2));
                    AddUntilStep("wait some hit", () => Player.ScoreProcessor.TotalScore.Value > 500000);

                    AddStep("seek to completion", () => Player.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));
                    AddUntilStep("wait for all hit", () => Player.ScoreProcessor.HasCompleted.Value);

                    AddStep("output statistics", () =>
                    {
                        var stats = Player.ScoreProcessor.Statistics;

                        File.AppendAllLines(filename, new[]
                        {
                            $"{Path.GetFileNameWithoutExtension(f)}\t{count(HitResult.Great)}\t{count(HitResult.Ok)}\t{count(HitResult.Meh)}\t{count(HitResult.Miss)}\t{Player.ScoreProcessor.Accuracy.Value}",
                        });

                        int count(HitResult result) => stats.TryGetValue(result, out int value) ? value : 0;
                    });
                }
            }
        }

        private void loadPlayerWithBeatmap(IBeatmap beatmap, Score score)
        {
            AddStep("create player", () =>
            {
                Logger.Log($"running for {score.ScoreInfo.OnlineID}");
                CreatePlayer(score, new OsuRuleset(), beatmap);
            });

            AddStep("Load player", () => LoadScreen(Player));
            AddUntilStep("player loaded", () => Player.IsLoaded);
        }

        protected void CreatePlayer(Score score, Ruleset ruleset, IBeatmap? beatmap = null)
        {
            SelectedMods.Value = score.ScoreInfo.Mods;

            Beatmap.Value = beatmap != null
                ? CreateWorkingBeatmap(beatmap)
                : CreateWorkingBeatmap(ruleset.RulesetInfo);

            Player = new TestReplayPlayer(score);
        }
    }
}
