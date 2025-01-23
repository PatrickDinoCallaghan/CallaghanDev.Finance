using System.Numerics;
using TALib.Helpers;


namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode MidPoint<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            MidPointImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int MidPointLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode MidPoint<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            MidPointImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode MidPointImpl<T>(
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

            if (optInTimePeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            /* Find the highest and lowest value of a time series over the period.
             *   MidPoint = (Highest Value + Lowest Value) / 2
             *
             * See MidPrice if the input is a price bar with a high and low time series.
             */

            var lookbackTotal = MidPointLookback(optInTimePeriod);
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
            while (today <= endIdx)
            {
                var lowest = inReal[trailingIdx++];
                var highest = lowest;
                for (var i = trailingIdx; i <= today; i++)
                {
                    var tmp = inReal[i];
                    if (tmp < lowest)
                    {
                        lowest = tmp;
                    }
                    else if (tmp > highest)
                    {
                        highest = tmp;
                    }
                }

                outReal[outIdx++] = (highest + lowest) / FunctionHelpers.Two<T>();
                today++;
            }

            // Keep the outBegIdx relative to the caller input before returning.
            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
