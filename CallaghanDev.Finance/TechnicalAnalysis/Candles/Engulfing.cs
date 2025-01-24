using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode Engulfing(
          ReadOnlySpan<float> inOpen,
          ReadOnlySpan<float> inHigh,
          ReadOnlySpan<float> inLow,
          ReadOnlySpan<float> inClose,
          Range inRange,
          Span<int> outIntType,
          out Range outRange)
        {
            return Candles.Engulfing<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode Engulfing<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            EngulfingImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int EngulfingLookback() => 2;

        
        
        

        private static Core.RetCode Engulfing<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            EngulfingImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode EngulfingImpl<T>(
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

            var lookbackTotal = EngulfingLookback();
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
             * outIntType is positive (100) when bullish or negative (-100) when bearish:
             *   - 100 is returned when the second candle's real body begins before and ends after the first candle's real body
             * it should be considered that an engulfing must appear in a downtrend if bullish or in an uptrend if bearish,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsEngulfingPattern(inOpen, inClose, i) ? (int)CandleHelpers.CandleColor(inClose, inOpen, i) * 100 : 0;

                i++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsEngulfingPattern<T>(ReadOnlySpan<T> inOpen, ReadOnlySpan<T> inClose, int i) where T : IFloatingPointIeee754<T> =>
            // white engulfs black
            CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.Black &&
            (inClose[i] >= inOpen[i - 1] && inOpen[i] < inClose[i - 1] || inClose[i] > inOpen[i - 1] && inOpen[i] <= inClose[i - 1])
            ||
            // black engulfs white
            CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.White &&
            (inOpen[i] >= inClose[i - 1] && inClose[i] < inOpen[i - 1] || inOpen[i] > inClose[i - 1] && inClose[i] <= inOpen[i - 1]);
    }

}
