using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Candles
    {
        public static Core.RetCode UpDownSideGapThreeMethods(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.UpDownSideGapThreeMethods<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode UpDownSideGapThreeMethods<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            UpDownSideGapThreeMethodsImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int UpDownSideGapThreeMethodsLookback() => 2;

        private static Core.RetCode UpDownSideGapThreeMethods<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            UpDownSideGapThreeMethodsImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode UpDownSideGapThreeMethodsImpl<T>(
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

            var lookbackTotal = UpDownSideGapThreeMethodsLookback();
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
             *   - first candle: white (black) candle
             *   - second candle: white (black) candle
             *   - upside (downside) gap between the first and the second real bodies
             *   - third candle: black (white) candle that opens within the second real body and closes within the first real body
             * outIntType is negative (-100): upside gap two crows is always bearish
             * it should be considered that up/downside gap three methods is significant when it appears in a trend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsUpDownSideGapThreeMethodsPattern(inOpen, inClose, i)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i - 2) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                i++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsUpDownSideGapThreeMethodsPattern<T>(ReadOnlySpan<T> inOpen, ReadOnlySpan<T> inClose, int i)
            where T : IFloatingPointIeee754<T> =>
            // 1st and 2nd of same color
            CandleHelpers.CandleColor(inClose, inOpen, i - 2) == CandleHelpers.CandleColor(inClose, inOpen, i - 1) &&
            // 3rd opposite color
            (int)CandleHelpers.CandleColor(inClose, inOpen, i - 1) == -(int)CandleHelpers.CandleColor(inClose, inOpen, i) &&
            // 3rd opens within 2nd rb
            inOpen[i] < T.Max(inClose[i - 1], inOpen[i - 1]) && inOpen[i] > T.Min(inClose[i - 1], inOpen[i - 1]) &&
            // 3rd closes within 1st rb
            inClose[i] < T.Max(inClose[i - 2], inOpen[i - 2]) && inClose[i] > T.Min(inClose[i - 2], inOpen[i - 2]) &&
            (
                // when 1st is white
                CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.White &&
                // upside gap
                CandleHelpers.RealBodyGapUp(inOpen, inClose, i - 1, i - 2)
                ||
                // when 1st is black
                CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.Black &&
                // downside gap
                CandleHelpers.RealBodyGapDown(inOpen, inClose, i - 1, i - 2)
            );
    }
}
