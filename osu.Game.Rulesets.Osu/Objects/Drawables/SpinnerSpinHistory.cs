// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    /// <summary>
    /// Stores the spinning history of a single spinner.<br />
    /// Instants of movement deltas may be added or removed from this in order to calculate the total rotation for the spinner.
    /// </summary>
    /// <remarks>
    /// A single, full rotation of the spinner is defined as a 360-degree rotation of the spinner, starting from 0, going in a single direction.<br />
    /// </remarks>
    /// <example>
    /// If the player spins 90-degrees clockwise, then changes direction, they need to spin 90-degrees counter-clockwise to return to 0
    /// and then continue rotating the spinner for another 360-degrees in the same direction.
    /// </example>
    public class SpinnerSpinHistory
    {
        /// <summary>
        /// The sum of all complete spins and any current partial spin, in degrees.
        /// </summary>
        /// <remarks>
        /// This is the final scoring value.
        /// </remarks>
        public float TotalRotation => 360 * completedSpins.Count + currentMaxRotation;

        private readonly Stack<CompletedSpin> completedSpins = new Stack<CompletedSpin>();

        /// <summary>
        /// The total accumulated rotation.
        /// </summary>
        private float totalAbsoluteRotation;

        private float totalAbsoluteRotationAtLastCompletion;

        /// <summary>
        /// For the current spin, represents the maximum rotation (from 0..360) achieved by the user.
        /// </summary>
        private float currentMaxRotation;

        /// <summary>
        /// The current spin, from -360..360.
        /// </summary>
        private float currentRotation => totalAbsoluteRotation - totalAbsoluteRotationAtLastCompletion;

        private double lastReportTime = double.NegativeInfinity;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="delta">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return;

            // TODO: Debug.Assert(Math.Abs(delta) < 180);
            // This will require important frame guarantees.

            totalAbsoluteRotation += delta;

            if (currentTime >= lastReportTime)
                addDelta(currentTime);
            else
                rewindDelta(currentTime);

            lastReportTime = currentTime;
        }

        private void addDelta(double currentTime)
        {
            currentMaxRotation = Math.Max(currentMaxRotation, Math.Abs(currentRotation));

            while (currentMaxRotation >= 360)
            {
                int direction = Math.Sign(currentRotation);

                completedSpins.Push(new CompletedSpin(currentTime, direction));
                totalAbsoluteRotationAtLastCompletion += direction * 360;
                currentMaxRotation = Math.Abs(currentRotation);
            }
        }

        private void rewindDelta(double currentTime)
        {
            while (completedSpins.TryPeek(out var segment) && segment.CompletionTime > currentTime)
            {
                completedSpins.Pop();
                totalAbsoluteRotationAtLastCompletion -= segment.Direction * 360;
                currentMaxRotation = Math.Abs(currentRotation);
            }

            currentMaxRotation = Math.Abs(currentRotation);
        }

        /// <summary>
        /// Represents a single completed spin.
        /// </summary>
        private class CompletedSpin
        {
            /// <summary>
            /// The time at which this spin completion occurred.
            /// </summary>
            public readonly double CompletionTime;

            /// <summary>
            /// The direction this spin completed in.
            /// </summary>
            public readonly int Direction;

            public CompletedSpin(double completionTime, int direction)
            {
                Debug.Assert(direction == -1 || direction == 1);

                CompletionTime = completionTime;
                Direction = direction;
            }
        }
    }
}
