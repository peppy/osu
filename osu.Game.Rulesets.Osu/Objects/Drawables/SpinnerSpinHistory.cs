// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public float TotalRotation => 360 * segments.Count(s => Math.Abs(s.LocalMax) == 360) + currentMaxRotation;

        /// <summary>
        /// The list of all segments where either:
        /// <list type="bullet">
        /// <item>The spinning direction was changed.</item>
        /// <item>A full spin of 360 degrees was performed in either direction.</item>
        /// </list>
        /// </summary>
        private readonly Stack<SpinSegment> segments = new Stack<SpinSegment>();

        private float totalRotation;

        private float currentRotation;
        private float currentMaxRotation;

        private double lastReportTime = double.NegativeInfinity;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="delta">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float delta)
        {
            totalRotation += delta;

            if (currentTime >= lastReportTime)
                addDelta(currentTime, delta);
            else
                rewindDelta(currentTime, delta);

            lastReportTime = currentTime;
        }

        int currentDirection;

        private void addDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return;

            currentRotation += delta;
            currentMaxRotation = Math.Max(currentMaxRotation, Math.Abs(currentRotation));

            int direction = Math.Sign(delta);

            while (currentMaxRotation >= 360)
            {
                segments.Push(new SpinSegment(currentTime, direction, direction * 360));

                currentRotation -= direction * 360;
                currentMaxRotation = Math.Abs(currentRotation);
            }

            if (currentDirection != direction)
            {
                segments.Push(new SpinSegment(currentTime, direction, currentMaxRotation));
                currentDirection = direction;
            }
        }

        private void rewindDelta(double currentTime, float delta)
        {
            while (segments.TryPeek(out var segment) && segment.StartTime > currentTime)
            {
                totalRotation -= 360 * segment.Direction;
                currentMaxRotation = segment.LocalMax;
                segments.Pop();
            }

            currentRotation = totalRotation % 360;
            currentMaxRotation = Math.Max(currentMaxRotation, Math.Abs(currentRotation));
        }

        /// <summary>
        /// Represents a single segment of history.
        /// </summary>
        /// <remarks>
        /// Each time the player changes direction, a new segment is recorded.
        /// A segment stores the current absolute angle of rotation. Generally this would be either -360 or 360 for a completed spin, or
        /// a number representing the last incomplete spin.
        /// </remarks>
        private class SpinSegment
        {
            /// <summary>
            /// The start time of this segment, when the direction change occurred.
            /// </summary>
            public readonly double StartTime;

            /// <summary>
            /// The direction this segment started in.
            /// </summary>
            public readonly int Direction;

            public readonly float LocalMax;

            public SpinSegment(double startTime, int direction, float localMax)
            {
                Debug.Assert(direction == -1 || direction == 1);

                StartTime = startTime;
                Direction = direction;
                LocalMax = localMax;
            }
        }
    }
}
