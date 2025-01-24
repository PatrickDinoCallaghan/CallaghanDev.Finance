using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode TasukiGap(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.TasukiGap<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode TasukiGap<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TasukiGapImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int TasukiGapLookback() => CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near) + 2;

        
        
        

        private static Core.RetCode TasukiGap<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TasukiGapImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode TasukiGapImpl<T>(
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

            var lookbackTotal = TasukiGapLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var nearPeriodTotal = T.Zero;
            var nearTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near);
            var i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 1);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - upside (downside) gap
             *   - first candle after the window: white (black) candlestick
             *   - second candle: black (white) candlestick that opens within the previous real body and closes under (above)
             *     the previous real body inside the gap
             *   - the size of two real bodies should be near the same
             * The meaning of "near" is specified with CandleSettings
             * outIntType is positive (100) when bullish or negative (-100) when bearish
             * it should be considered that tasuki gap is significant when it appears in a trend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsTasukiGapPattern(inOpen, inHigh, inLow, inClose, i, nearPeriodTotal)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i - 1) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                nearPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearTrailingIdx - 1);

                i++;
                nearTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsTasukiGapPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T nearPeriodTotal) where T : IFloatingPointIeee754<T> =>
            (
                // upside gap
                CandleHelpers.RealBodyGapUp(inOpen, inClose, i - 1, i - 2) &&
                // 1st: white
                CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.White &&
                // 2nd: black
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
                // that opens within the white real body
                inOpen[i] < inClose[i - 1] && inOpen[i] > inOpen[i - 1] &&
                // and closes under the white real body
                inClose[i] < inOpen[i - 1] &&
                // inside the gap
                inClose[i] > T.Max(inClose[i - 2], inOpen[i - 2]) &&
                // size of 2 real body near the same
                T.Abs(CandleHelpers.RealBody(inClose, inOpen, i - 1) - CandleHelpers.RealBody(inClose, inOpen, i)) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal, i - 1)
            )
            ||
            (
                // downside gap
                CandleHelpers.RealBodyGapDown(inOpen, inClose, i - 1, i - 2) &&
                // 1st: black
                CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.Black &&
                // 2nd: white
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
                // that opens within the black rb
                inOpen[i] < inOpen[i - 1] && inOpen[i] > inClose[i - 1] &&
                // and closes above the black rb
                inClose[i] > inOpen[i - 1] &&
                // inside the gap
                inClose[i] < T.Min(inClose[i - 2], inOpen[i - 2]) &&
                // size of 2 real body near the same
                T.Abs(CandleHelpers.RealBody(inClose, inOpen, i - 1) - CandleHelpers.RealBody(inClose, inOpen, i)) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal, i - 1)
            );
    }

}

