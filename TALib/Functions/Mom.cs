using System.Numerics;

namespace TALib
{

    public static partial class Functions
    {
        public static Core.RetCode Mom<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
            MomImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int MomLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

        private static Core.RetCode Mom<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
            MomImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode MomImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            // The Mom function is the only one who is not normalized, and thus
            // should be avoided for comparing different time series of prices.

            var lookbackTotal = MomLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Calculate Momentum. Just subtract the value from 'period' ago from current value.
            int outIdx = default;
            var inIdx = startIdx;
            var trailingIdx = startIdx - lookbackTotal;
            while (inIdx <= endIdx)
            {
                outReal[outIdx++] = inReal[inIdx++] - inReal[trailingIdx++];
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
