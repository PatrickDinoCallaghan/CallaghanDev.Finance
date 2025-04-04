using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode PlusDM<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            PlusDMImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);


        public static int PlusDMLookback(int optInTimePeriod = 14) => optInTimePeriod switch
        {
            < 1 => -1,
            > 1 => optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.PlusDM) - 1,
            _ => 1
        };

        private static Core.RetCode PlusDM<T>(
            T[] inHigh,
            T[] inLow,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            PlusDMImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode PlusDMImpl<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            /* The DM1 (one period) is based on the largest part of today's range that is outside of yesterday's range.
             *
             * The following 7 cases explain how the +DM and -DM are calculated on one period:
             *
             * Case 1:                       Case 2:
             *    C│                        A│
             *     │                         │ C│
             *     │ +DM1 = (C-A)           B│  │ +DM1 = 0
             *     │ -DM1 = 0                   │ -DM1 = (B-D)
             * A│  │                           D│
             *  │ D│
             * B│
             *
             * Case 3:                       Case 4:
             *    C│                           C│
             *     │                        A│  │
             *     │ +DM1 = (C-A)            │  │ +DM1 = 0
             *     │ -DM1 = 0               B│  │ -DM1 = (B-D)
             * A│  │                            │
             *  │  │                           D│
             * B│  │
             *    D│
             *
             * Case 5:                      Case 6:
             * A│                           A│ C│
             *  │ C│ +DM1 = 0                │  │  +DM1 = 0
             *  │  │ -DM1 = 0                │  │  -DM1 = 0
             *  │ D│                         │  │
             * B│                           B│ D│
             *
             *
             * Case 7:
             *
             *    C│
             * A│  │
             *  │  │ +DM1=0
             * B│  │ -DM1=0
             *    D│
             *
             * In case 3 and 4, the rule is that the smallest delta between (C-A) and (B-D) determine
             * which of +DM or -DM is zero.
             *
             * In case 7, (C-A) and (B-D) are equal, so both +DM and -DM are zero.
             *
             * The rules remain the same when A=B and C=D (when the highs equal the lows).
             *
             * When calculating the DM over a period > 1, the one-period DM for the desired period are initially summed.
             * In other words, for a -DM14, sum the -DM1 for the first 14 days
             * (that's 13 values because there is no DM for the first day!)
             * Subsequent DM are calculated using Wilder's smoothing approach:
             *
             *                                     Previous +DM14
             *   Today's +DM14 = Previous +DM14 -  ────────────── + Today's +DM1
             *                                           14
             *
             * Calculation of a +DI14 is as follows:
             *
             *             +DM14
             *   +DI14 =  ────────
             *              TR14
             *
             * Calculation of the TR14 is:
             *
             *                                  Previous TR14
             *   Today's TR14 = Previous TR14 - ────────────── + Today's TR1
             *                                        14
             *
             *   The first TR14 is the summation of the first 14 TR1. See the TRange function on how to calculate the true range.
             *
             * Reference:
             *    New Concepts In Technical Trading Systems, J. Welles Wilder Jr
             */

            var lookbackTotal = PlusDMLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Trap the case where no smoothing is needed.
            if (optInTimePeriod == 1)
            {
                // No smoothing needed. Just do a simple DM1 for each price bar.
                return CalcPlusDMForPeriodOne(inHigh, inLow, startIdx, endIdx, outReal, out outRange);
            }

            var outBegIdx = startIdx;
            var today = startIdx - lookbackTotal;

            var timePeriod = T.CreateChecked(optInTimePeriod);
            T prevPlusDM = T.Zero, _ = T.Zero;

            FunctionHelpers.InitDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, out var prevHigh, ref today, out var prevLow, out var _,
                timePeriod, ref prevPlusDM, ref _, ref _);

            // Process subsequent DM

            // Skip the unstable period.
            for (var i = 0; i < Core.UnstablePeriodSettings.Get(Core.UnstableFunc.PlusDM); i++)
            {
                today++;
                FunctionHelpers.UpdateDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, ref today, ref prevHigh, ref prevLow, ref _, ref prevPlusDM,
                    ref _, ref _, timePeriod);
            }

            outReal[0] = prevPlusDM;
            var outIdx = 1;

            while (today < endIdx)
            {
                today++;
                FunctionHelpers.UpdateDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, ref today, ref prevHigh, ref prevLow, ref _, ref prevPlusDM,
                    ref _, ref _, timePeriod);
                outReal[outIdx++] = prevPlusDM;
            }

            outRange = new Range(outBegIdx, outBegIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static Core.RetCode CalcPlusDMForPeriodOne<T>(
            ReadOnlySpan<T> high,
            ReadOnlySpan<T> low,
            int startIdx,
            int endIdx,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            var today = startIdx - 1;
            var prevHigh = high[today];
            var prevLow = low[today];
            var outIdx = 0;

            while (today < endIdx)
            {
                today++;
                var (diffP, diffM) = FunctionHelpers.CalcDeltas(high, low, today, ref prevHigh, ref prevLow);
                outReal[outIdx++] = diffP > T.Zero && diffP > diffM ? diffP : T.Zero;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}