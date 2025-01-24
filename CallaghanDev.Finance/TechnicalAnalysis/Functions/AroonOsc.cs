using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode AroonOsc<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AroonOscImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);


        public static int AroonOscLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod;

        
        
        

        private static Core.RetCode AroonOsc<T>(
            T[] inHigh,
            T[] inLow,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AroonOscImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode AroonOscImpl<T>(
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

            if (optInTimePeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            /* This code is almost identical to the Aroon function except that
             * instead of outputting AroonUp and AroonDown individually, an oscillator is build from both.
             *
             *   AroonOsc = AroonUp - AroonDown
             *
             */

            // This function is using a speed optimized algorithm for the min/max logic.
            //It might be needed to first look at how Min/Max works and this function will become easier to understand.

            var lookbackTotal = AroonOscLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Proceed with the calculation for the requested range.
            // The algorithm allows the input and output to be the same buffer.
            int outIdx = default;
            var today = startIdx;
            var trailingIdx = startIdx - lookbackTotal;

            int highestIdx = -1, lowestIdx = -1;
            T highest = T.Zero, lowest = T.Zero;
            var factor = FunctionHelpers.Hundred<T>() / T.CreateChecked(optInTimePeriod);
            while (today <= endIdx)
            {
                (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
                (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

                /* The oscillator is the following:
                 *   AroonUp   = factor * (optInTimePeriod - (today - highestIdx))
                 *   AroonDown = factor * (optInTimePeriod - (today - lowestIdx))
                 *   AroonOsc  = AroonUp - AroonDown
                 *
                 * An arithmetic simplification gives:
                 *   Aroon = factor * (highestIdx - lowestIdx)
                 */
                var arron = factor * T.CreateChecked(highestIdx - lowestIdx);

                //Input and output buffer can be the same, so writing to the output is the last thing being done here.
                outReal[outIdx++] = arron;

                trailingIdx++;
                today++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}
