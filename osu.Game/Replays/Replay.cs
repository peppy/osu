// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Lists;
using osu.Game.Rulesets.Replays;
using osu.Game.Utils;

namespace osu.Game.Replays
{
    public class Replay : IDeepCloneable<Replay>
    {
        /// <summary>
        /// Whether all frames for this replay have been received.
        /// If false, gameplay would be paused to wait for further data, for instance.
        /// </summary>
        public bool HasReceivedAllFrames = true;

        public ReplayFrameList Frames = new ReplayFrameList();

        public Replay DeepClone()
        {
            var frames = new ReplayFrameList();
            frames.AddRange(Frames);

            return new Replay
            {
                HasReceivedAllFrames = HasReceivedAllFrames,
                // individual frames are mutable for now but hopefully this will not be a thing in the future.
                Frames = frames,
            };
        }
    }

    public class ReplayFrameList : SortedList<ReplayFrame>
    {
        public ReplayFrameList()
            : base(replay_frame_comparer)
        {
        }

        public ReplayFrameList(IEnumerable<ReplayFrame> frames)
            : base(replay_frame_comparer)
        {
            AddRange(frames);
        }

        public static implicit operator ReplayFrameList(List<ReplayFrame> frames) => new ReplayFrameList(frames);

        private class ReplayFrameComparer : IComparer<ReplayFrame>
        {
            public int Compare(ReplayFrame? f1, ReplayFrame? f2)
            {
                ArgumentNullException.ThrowIfNull(f1);
                ArgumentNullException.ThrowIfNull(f2);

                int compare = f1.Time.CompareTo(f2.Time);
                if (compare != 0)
                    return compare;

                // stable sorting
                return -1;
            }
        }

        private static readonly IComparer<ReplayFrame> replay_frame_comparer = new ReplayFrameComparer();
    }
}
