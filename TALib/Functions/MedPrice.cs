using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {

        public static Core.RetCode MedPrice<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            MedPriceImpl(inHigh, inLow, inRange, outReal, out outRange);


        public static int MedPriceLookback() => 0;

        private static Core.RetCode MedPrice<T>(
            T[] inHigh,
            T[] inLow,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            MedPriceImpl<T>(inHigh, inLow, inRange, outReal, out outRange);

        private static Core.RetCode MedPriceImpl<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            /* MedPrice = (High + Low ) / 2
             * This is the high and low of the same price bar.
             *
             * See MidPrice to use instead the highest high and lowest low over multiple price bar.
             */

            int outIdx = default;
            for (var i = startIdx; i <= endIdx; i++)
            {
                outReal[outIdx++] = (inHigh[i] + inLow[i]) / FunctionHelpers.Two<T>();
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}