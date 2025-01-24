using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode MinIndex<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<int> outInteger,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            MinIndexImpl(inReal, inRange, outInteger, out outRange, optInTimePeriod);


        public static int MinIndexLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;
        private static Core.RetCode MinIndex<T>(
            T[] inReal,
            Range inRange,
            int[] outInteger,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            MinIndexImpl<T>(inReal, inRange, outInteger, out outRange, optInTimePeriod);

        private static Core.RetCode MinIndexImpl<T>(
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

            var lookbackTotal = MinIndexLookback(optInTimePeriod);
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

            var lowestIdx = -1;
            var lowest = T.Zero;
            while (today <= endIdx)
            {
                (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

                outInteger[outIdx++] = lowestIdx;
                trailingIdx++;
                today++;
            }

            // Keep the outBegIdx relative to the caller input before returning.
            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
