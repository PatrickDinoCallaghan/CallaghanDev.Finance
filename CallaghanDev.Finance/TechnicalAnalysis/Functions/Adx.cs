using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        /// <summary>
        /// Average Directional Movement Index (Momentum Indicators)
        /// </summary>
        /// <typeparam name="T">
        /// The numeric data type, typically <see cref="float"/> or <see cref="double"/>,
        /// implementing the <see cref="IFloatingPointIeee754{T}"/> interface.
        /// </typeparam>
        /// <param name="inHigh">A span of input high prices.</param>
        /// <param name="inLow">A span of input low prices.</param>
        /// <param name="inClose">A span of input close prices.</param>
        /// <param name="inRange">A range of indices that determines the portion of data to be calculated within the input spans.</param>
        /// <param name="outReal">The span in which to store the calculated values.</param>
        /// <param name="outRange">The range of indices representing the valid values within the output span.</param>
        /// <param name="optInTimePeriod">The time period.</param>
        /// <returns>
        /// A <see cref="Core.RetCode"/> value indicating the success or failure of the calculation.
        /// Returns <see cref="Core.RetCode.Success"/> on successful calculation, or an appropriate error code otherwise.
        /// </returns>
        
        /// The function calculates the Average Directional Index, which is a measure of trend strength.
        /// It does not indicate trend direction, only the strength of the trend.
        /// The ADX is derived from the smoothed averages of the difference between +DI and -DI.
        /// <para>
        /// The ADX calculation involves several steps:
        /// <list type="number">
        /// <item>
        /// <description>Calculate the True Range (TR), +DM (Directional Movement), and -DM for the specified time period.</description>
        /// </item>
        /// <item>
        /// <description>Compute the smoothed averages of +DM and -DM, and then calculate the +DI and -DI.</description>
        /// </item>
        /// <item>
        /// <description>Calculate the DX (Directional Index) from the difference between +DI and -DI.</description>
        /// </item>
        /// <item>
        /// <description>Finally, compute the ADX as the smoothed average of the DX values.</description>
        /// </item>
        /// </list>
        /// </para>
        public static Core.RetCode Adx<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AdxImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

        /// <summary>
        /// Returns the lookback period for <see cref="Adx{T}"/>.
        /// </summary>
        /// <param name="optInTimePeriod">The time period.</param>
        /// <returns>The number of periods required before the first output value can be calculated.</returns>

        public static int AdxLookback(int optInTimePeriod = 14) =>
            optInTimePeriod < 2 ? -1 : optInTimePeriod * 2 + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Adx) - 1;

        private static Core.RetCode Adx<T>(
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AdxImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode AdxImpl<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inClose.Length) is not
                { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 2)
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
             *  │  │ +DM=0
             * B│  │ -DM=0
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
             *                                    Previous -DM14
             *   Today's -DM14 = Previous -DM14 -  ────────────── + Today's -DM1
             *                                          14
             *
             * (Same thing for +DM14)
             *
             * Calculation of a -DI14 is as follows:
             *
             *             -DM14
             *   -DI14 =  ────────
             *              TR14
             *
             * (Same thing for +DI14)
             *
             * Calculation of the TR14 is:
             *
             *                                  Previous TR14
             *   Today's TR14 = Previous TR14 - ───────────── + Today's TR1
             *                                        14
             *
             *   The first TR14 is the summation of the first 14 TR1. See the TRange function on how to calculate the true range.
             *
             * Calculation of the DX14 is:
             *
             *   diffDI = ABS((-DI14) - (+DI14))
             *   sumDI  = (-DI14) + (+DI14)
             *
             *   DX14 = 100 * (diffDI / sumDI)
             *
             * Calculation of the first ADX:
             *
             *   ADX14 = SUM of the first 14 DX
             *
             * Calculation of subsequent ADX:
             *
             *           ((Previous ADX14) * (14 - 1)) + Today's DX
             *   ADX14 = ──────────────────────────────────────────
             *                            14
             *
             * Reference:
             *   New Concepts In Technical Trading Systems, J. Welles Wilder Jr
             */

            var lookbackTotal = AdxLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);
            T prevMinusDM, prevPlusDM;
            var prevTR = prevMinusDM = prevPlusDM = T.Zero;
            var today = startIdx;
            var outBegIdx = today;
            today = startIdx - lookbackTotal;

            FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
                ref prevPlusDM, ref prevMinusDM, ref prevTR);

            var sumDX = AddAllInitialDX(inHigh, inLow, inClose, timePeriod, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR);

            // Calculate the first ADX
            var prevADX = sumDX / timePeriod;

            SkipAdxUnstablePeriod(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM, ref prevMinusDM,
                ref prevTR, timePeriod, ref prevADX);

            // Output the first ADX
            outReal[0] = prevADX;
            var outIdx = 1;

            CalcAndOutputSubsequentADX(inHigh, inLow, inClose, outReal, ref today, endIdx, ref prevHigh, ref prevLow, ref prevClose,
                ref prevPlusDM, ref prevMinusDM, ref prevTR, timePeriod, ref prevADX, ref outIdx);

            outRange = new Range(outBegIdx, outBegIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static T AddAllInitialDX<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            T timePeriod,
            ref int today,
            ref T prevHigh,
            ref T prevLow,
            ref T prevClose,
            ref T prevPlusDM,
            ref T prevMinusDM,
            ref T prevTR) where T : IFloatingPointIeee754<T>
        {
            var sumDX = T.Zero;
            for (var i = Int32.CreateTruncating(timePeriod); i > 0; i--)
            {
                today++;
                FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                    ref prevMinusDM, ref prevTR, timePeriod);

                if (T.IsZero(prevTR))
                {
                    continue;
                }

                var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                var tempReal = minusDI + plusDI;
                if (!T.IsZero(tempReal))
                {
                    sumDX += FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal);
                }
            }

            return sumDX;
        }

        private static void SkipAdxUnstablePeriod<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> icClose,
            ref int today,
            ref T prevHigh,
            ref T prevLow,
            ref T prevClose,
            ref T prevPlusDM,
            ref T prevMinusDM,
            ref T prevTR,
            T timePeriod,
            ref T prevADX) where T : IFloatingPointIeee754<T>
        {
            for (var i = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Adx); i > 0; i--)
            {
                today++;
                FunctionHelpers.UpdateDMAndTR(inHigh, inLow, icClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                    ref prevMinusDM, ref prevTR, timePeriod);

                if (T.IsZero(prevTR))
                {
                    continue;
                }

                var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                var tempReal = minusDI + plusDI;
                if (T.IsZero(tempReal))
                {
                    continue;
                }

                tempReal = FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal);
                prevADX = (prevADX * (timePeriod - T.One) + tempReal) / timePeriod;
            }
        }

        private static void CalcAndOutputSubsequentADX<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Span<T> outReal,
            ref int today,
            int endIdx,
            ref T prevHigh,
            ref T prevLow,
            ref T prevClose,
            ref T prevPlusDM,
            ref T prevMinusDM,
            ref T prevTR,
            T timePeriod,
            ref T prevADX,
            ref int outIdx) where T : IFloatingPointIeee754<T>
        {
            while (today < endIdx)
            {
                today++;
                FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                    ref prevMinusDM, ref prevTR, timePeriod);

                if (!T.IsZero(prevTR))
                {
                    var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                    var tempReal = minusDI + plusDI;
                    if (!T.IsZero(tempReal))
                    {
                        tempReal = FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal);
                        prevADX = (prevADX * (timePeriod - T.One) + tempReal) / timePeriod;
                    }
                }

                outReal[outIdx++] = prevADX;
            }
        }
    }

}
