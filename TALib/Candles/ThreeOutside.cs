using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Candles
    {
        public static Core.RetCode ThreeOutside(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.ThreeOutside<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode ThreeOutside<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            ThreeOutsideImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int ThreeOutsideLookback() => 3;

        
        
        

        private static Core.RetCode ThreeOutside<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            ThreeOutsideImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode ThreeOutsideImpl<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inOpen.Length, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            var lookbackTotal = ThreeOutsideLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - first: black (white) real body
             *   - second: white (black) real body that engulfs the prior real body
             *   - third: candle that closes higher (lower) than the second candle
             * outIntType is positive (100) for the three outside up or negative (-100) for the three outside down
             * it should be considered that a three outside up must appear in a downtrend and three outside down must appear in an uptrend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsThreeOutsidePattern(inOpen, inClose, i)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i - 1) * 100
                    : 0;

                i++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsThreeOutsidePattern<T>(ReadOnlySpan<T> inOpen, ReadOnlySpan<T> inClose, int i)
            where T : IFloatingPointIeee754<T> =>
            // white engulfs black
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.White &&
            CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.Black &&
            inClose[i - 1] > inOpen[i - 2] && inOpen[i - 1] < inClose[i - 2] &&
            // third candle higher
            inClose[i] > inClose[i - 1]
            ||
            // black engulfs white
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.Black &&
            CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.White &&
            inOpen[i - 1] > inClose[i - 2] && inClose[i - 1] < inOpen[i - 2] &&
            // third candle lower
            inClose[i] < inClose[i - 1];
    }
}
