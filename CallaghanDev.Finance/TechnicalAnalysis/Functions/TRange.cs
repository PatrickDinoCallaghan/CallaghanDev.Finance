using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode TRange<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TRangeImpl(inHigh, inLow, inClose, inRange, outReal, out outRange);


        public static int TRangeLookback() => 1;

        private static Core.RetCode TRange<T>(
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TRangeImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange);

        private static Core.RetCode TRangeImpl<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            /* True Range is the greatest of the following:
             *
             *   val1 = distance from today's high to today's low.
             *   val2 = distance from yesterday's close to today's high.
             *   val3 = distance from yesterday's close to today's low.
             *
             * Some books and software makes the first TR value to be the (high - low) of the first bar.
             * This function instead ignores the first price bar, and only outputs starting at the second price bar are valid.
             * This is done for avoiding inconsistency.
             */

            var lookbackTotal = TRangeLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            int outIdx = default;
            var today = startIdx;
            while (today <= endIdx)
            {
                var tempHT = inHigh[today];
                var tempLT = inLow[today];
                var tempCY = inClose[today - 1];

                outReal[outIdx++] = FunctionHelpers.TrueRange(tempHT, tempLT, tempCY);
                today++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}