// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : HarmonicSkill
    {
        private readonly bool hasHiddenMod;

        public Reading(IBeatmap beatmap, Mod[] mods)
            : base(beatmap, mods)
        {
            hasHiddenMod = mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value);
        }

        private double currentStrain;

        private const double skill_multiplier = 2.5;
        private const double strain_decay_base = 0.8;

        private double strainDecay(double ms) => Math.Pow(strain_decay_base, ms / 1000);

        protected override double ObjectDifficultyOf(DifficultyHitObject current)
        {
            double decay = strainDecay(current.DeltaTime);

            currentStrain *= decay;
            currentStrain += calculateAdjustedDifficulty(current) * (1 - decay) * skill_multiplier;

            return currentStrain;
        }

        private double calculateAdjustedDifficulty(DifficultyHitObject current)
        {
            double difficulty = ReadingEvaluator.EvaluateDifficultyOf(current, hasHiddenMod);

            if (Mods.Any(m => m is OsuModTouchDevice))
                difficulty = Math.Pow(difficulty, 0.89);

            if (Mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = Mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                difficulty *= 1.0 - magnetisedStrength;
            }

            if (Mods.Any(m => m is OsuModRelax))
                difficulty *= 0.4;

            if (Mods.Any(m => m is OsuModAutopilot))
                difficulty *= 0.1;

            difficulty *= 0.825 + Math.Pow(Math.Max(0, ((OsuDifficultyHitObject)current).OverallDifficulty), 2.2) / 1125.0;

            return difficulty;
        }

        protected override void ApplyDifficultyTransformation(double[] difficulties)
        {
            const double reduced_difficulty_base_line = 0.0; // Assume the first seconds are completely memorised

            int reducedNoteCount = calculateReducedNoteCount();

            for (int i = 0; i < Math.Min(difficulties.Length, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((double)i / reducedNoteCount, 0, 1)));
                difficulties[i] *= Interpolation.Lerp(reduced_difficulty_base_line, 1.0, scale);
            }
        }

        private int calculateReducedNoteCount()
        {
            double clockRate = ModUtils.CalculateRateWithMods(Mods);
            const double reduced_difficulty_duration = 60 * 1000;

            if (Beatmap.HitObjects.Count == 0)
                return 0;

            double reducedDuration = Beatmap.HitObjects.First().StartTime + reduced_difficulty_duration * clockRate;

            int reducedNoteCount = 0;

            foreach (var hitObject in Beatmap.HitObjects.Skip(1))
            {
                if (hitObject.StartTime > reducedDuration)
                    break;

                reducedNoteCount++;
            }

            return reducedNoteCount;
        }

        public override double CountTopWeightedObjectDifficulties(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            if (NoteWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / NoteWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopNote == 0)
                return 0;

            return ObjectDifficulties.Sum(d => DifficultyCalculationUtils.Logistic(d / consistentTopNote, 1.15, 5, 1.1));
        }
    }
}
