﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public class BindableBeatDivisor : BindableNumber<int>
    {
        private DivisorPanelType panel = DivisorPanelType.Normal;

        public int LastNormalDivisor = 1;
        public int LastTripletDivisor = 1;
        public int LastOtherDivisor = 1;

        public static readonly int[] VALID_DIVISORS = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 16, 18, 24, 32 };
        public static readonly int[] VALID_NORMAL_DIVISORS = { 1, 2, 4, 8, 16, 32 };
        public static readonly int[] VALID_TRIPLET_DIVISORS = { 1, 3, 6, 9, 12, 18, 24 };
        public static readonly int[] VALID_OTHER_DIVISORS = { 1, 5, 7, 11 };

        /// <summary>An event that occurs when the <see cref="ValidDivisors"/> property has changed.</summary>
        public event Action ValidDivisorsChanged;

        public BindableBeatDivisor(int value = 1)
            : base(value)
        {
        }

        /// <summary>Triggers the <see cref="ValidDivisorsChanged"/> event.</summary>
        public virtual void TriggerValidDivisorsChanged()
        {
            ValidDivisorsChanged?.Invoke();
        }

        public void NextPanel()
        {
            Panel = (DivisorPanelType)((int)(panel + 1) % 3);
        }

        public void PreviousPanel()
        {
            Panel = (DivisorPanelType)((int)(panel + 2) % 3);
        }

        public void NextDivisor()
        {
            switch (panel)
            {
                case DivisorPanelType.Normal:
                    NextNormalDivisor();
                    return;

                case DivisorPanelType.Triplets:
                    NextTripletDivisor();
                    return;

                case DivisorPanelType.Other:
                    NextOtherDivisor();
                    return;

                default:
                    return;
            }
        }

        public void PreviousDivisor()
        {
            switch (panel)
            {
                case DivisorPanelType.Normal:
                    PreviousNormalDivisor();
                    return;

                case DivisorPanelType.Triplets:
                    PreviousTripletDivisor();
                    return;

                case DivisorPanelType.Other:
                    PreviousOtherDivisor();
                    return;

                default:
                    return;
            }
        }

        public void NextNormalDivisor() => Value = VALID_NORMAL_DIVISORS[Math.Min(VALID_NORMAL_DIVISORS.Length - 1, Array.IndexOf(VALID_NORMAL_DIVISORS, Value) + 1)];

        public void PreviousNormalDivisor() => Value = VALID_NORMAL_DIVISORS[Math.Max(0, Array.IndexOf(VALID_NORMAL_DIVISORS, Value) - 1)];

        public void NextTripletDivisor() => Value = VALID_TRIPLET_DIVISORS[Math.Min(VALID_TRIPLET_DIVISORS.Length - 1, Array.IndexOf(VALID_TRIPLET_DIVISORS, Value) + 1)];

        public void PreviousTripletDivisor() => Value = VALID_TRIPLET_DIVISORS[Math.Max(0, Array.IndexOf(VALID_TRIPLET_DIVISORS, Value) - 1)];

        public void NextOtherDivisor() => Value = VALID_OTHER_DIVISORS[Math.Min(VALID_OTHER_DIVISORS.Length - 1, Array.IndexOf(VALID_OTHER_DIVISORS, Value) + 1)];

        public void PreviousOtherDivisor() => Value = VALID_OTHER_DIVISORS[Math.Max(0, Array.IndexOf(VALID_OTHER_DIVISORS, Value) - 1)];

        public override int Value
        {
            get => base.Value;
            set
            {
                if (!VALID_DIVISORS.Contains(value))
                    throw new ArgumentOutOfRangeException($"Provided divisor is not in {nameof(VALID_DIVISORS)}");

                base.Value = value;
            }
        }

        public DivisorPanelType Panel
        {
            get => panel;
            set
            {
                switch (panel)
                {
                    case DivisorPanelType.Normal:
                        LastNormalDivisor = Value;
                        goto default;

                    case DivisorPanelType.Triplets:
                        LastTripletDivisor = Value;
                        goto default;

                    case DivisorPanelType.Other:
                        LastOtherDivisor = Value;
                        goto default;

                    default:
                        TriggerValidDivisorsChanged();
                        break;
                }

                panel = value;

                switch (panel)
                {
                    case DivisorPanelType.Normal:
                        ValidDivisors = VALID_NORMAL_DIVISORS;
                        base.Value = LastNormalDivisor;
                        goto default;

                    case DivisorPanelType.Triplets:
                        ValidDivisors = VALID_TRIPLET_DIVISORS;
                        base.Value = LastTripletDivisor;
                        goto default;

                    case DivisorPanelType.Other:
                        ValidDivisors = VALID_OTHER_DIVISORS;
                        base.Value = LastOtherDivisor;
                        goto default;

                    default:
                        TriggerValidDivisorsChanged();
                        return;
                }
            }
        }

        public int[] ValidDivisors = VALID_NORMAL_DIVISORS;

        protected override int DefaultMinValue => VALID_DIVISORS.First();
        protected override int DefaultMaxValue => VALID_DIVISORS.Last();
        protected override int DefaultPrecision => 1;

        /// <summary>
        /// Retrieves the appropriate colour for a beat divisor.
        /// </summary>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <param name="colours">The set of colours.</param>
        /// <returns>The applicable colour from <paramref name="colours"/> for <paramref name="beatDivisor"/>.</returns>
        public static Color4 GetColourFor(int beatDivisor, OsuColour colours)
        {
            switch (beatDivisor)
            {
                case 1:
                    return Color4.White;

                case 2:
                    return colours.Red;

                case 4:
                    return colours.Blue;

                case 8:
                    return colours.Yellow;

                case 16:
                    return colours.PurpleDark;

                case 3:
                    return colours.Purple;

                case 6:
                    return colours.YellowDark;

                case 12:
                    return colours.YellowDarker;

                default:
                    return Color4.Red;
            }
        }

        /// <summary>
        /// Retrieves the applicable divisor for a specific beat index.
        /// </summary>
        /// <param name="index">The 0-based beat index.</param>
        /// <param name="beatDivisor">The beat divisor.</param>
        /// <returns>The applicable divisor.</returns>
        public static int GetDivisorForBeatIndex(int index, int beatDivisor)
        {
            int beat = index % beatDivisor;

            foreach (int divisor in BindableBeatDivisor.VALID_DIVISORS)
            {
                if ((beat * divisor) % beatDivisor == 0)
                    return divisor;
            }

            return 0;
        }
    }

    public enum DivisorPanelType
    {
        Normal = 0,
        Triplets = 1,
        Other = 2
    }
}
