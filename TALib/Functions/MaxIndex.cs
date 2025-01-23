using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode MaxIndex<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<int> outInteger,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            MaxIndexImpl(inReal, inRange, outInteger, out outRange, optInTimePeriod);


        public static int MaxIndexLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode MaxIndex<T>(
            T[] inReal,
            Range inRange,
            int[] outInteger,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            MaxIndexImpl<T>(inReal, inRange, outInteger, out outRange, optInTimePeriod);

        private static Core.RetCode MaxIndexImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<int> outInteger,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            var lookbackTotal = MaxIndexLookback(optInTimePeriod);
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

            var highestIdx = -1;
            var highest = T.Zero;
            while (today <= endIdx)
            {
                (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);

                outInteger[outIdx++] = highestIdx;
                trailingIdx++;
                today++;
            }

            // Keep the outBegIdx relative to the caller input before returning.
            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}