using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {

        public static Core.RetCode MidPrice<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            MidPriceImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);


        public static int MidPriceLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode MidPrice<T>(
            T[] inHigh,
            T[] inLow,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            MidPriceImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode MidPriceImpl<T>(
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

            /* MidPrice = (Highest High + Lowest Low) / 2
             *
             * This function is equivalent to MedPrice when the period is 1.
             */

            var lookbackTotal = MidPriceLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            int outIdx = default;
            var today = startIdx;
            var trailingIdx = startIdx - lookbackTotal;
            while (today <= endIdx)
            {
                var lowest = inLow[trailingIdx];
                var highest = inHigh[trailingIdx++];
                for (var i = trailingIdx; i <= today; i++)
                {
                    var tmp = inLow[i];
                    if (tmp < lowest)
                    {
                        lowest = tmp;
                    }

                    tmp = inHigh[i];
                    if (tmp > highest)
                    {
                        highest = tmp;
                    }
                }

                outReal[outIdx++] = (highest + lowest) / FunctionHelpers.Two<T>();
                today++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
