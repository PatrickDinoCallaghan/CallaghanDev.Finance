using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Aroon<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outAroonDown,
            Span<T> outAroonUp,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AroonImpl(inHigh, inLow, inRange, outAroonDown, outAroonUp, out outRange, optInTimePeriod);


        public static int AroonLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod;

        private static Core.RetCode Aroon<T>(
            T[] inHigh,
            T[] inLow,
            Range inRange,
            T[] outAroonDown,
            T[] outAroonUp,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AroonImpl<T>(inHigh, inLow, inRange, outAroonDown, outAroonUp, out outRange, optInTimePeriod);

        private static Core.RetCode AroonImpl<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outAroonDown,
            Span<T> outAroonUp,
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

            // This function is using a speed optimized algorithm for the min/max logic.
            // It might be needed to first look at how Min/Max works and this function will become easier to understand.

            var lookbackTotal = AroonLookback(optInTimePeriod);
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

                outAroonUp[outIdx] = factor * T.CreateChecked(optInTimePeriod - (today - highestIdx));
                outAroonDown[outIdx] = factor * T.CreateChecked(optInTimePeriod - (today - lowestIdx));

                outIdx++;
                trailingIdx++;
                today++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}