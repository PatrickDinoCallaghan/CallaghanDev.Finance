using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode GapSideBySideWhiteLines(
          ReadOnlySpan<float> inOpen,
          ReadOnlySpan<float> inHigh,
          ReadOnlySpan<float> inLow,
          ReadOnlySpan<float> inClose,
          Range inRange,
          Span<int> outIntType,
          out Range outRange)
        {
            return Candles.GapSideBySideWhiteLines<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode GapSideBySideWhiteLines<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            GapSideBySideWhiteLinesImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int GapSideBySideWhiteLinesLookback() =>
            Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Equal)) + 2;

        
        
        

        private static Core.RetCode GapSideBySideWhiteLines<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            GapSideBySideWhiteLinesImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode GapSideBySideWhiteLinesImpl<T>(
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

            var lookbackTotal = GapSideBySideWhiteLinesLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var nearPeriodTotal = T.Zero;
            var equalPeriodTotal = T.Zero;
            var nearTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near);
            var equalTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Equal);
            var i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 1);
                i++;
            }

            i = equalTrailingIdx;
            while (i < startIdx)
            {
                equalPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, i - 1);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - upside or downside gap (between the bodies)
             *   - first candle after the window: white candlestick
             *   - second candle after the window: white candlestick with similar size (near the same) and
             *     about the same open (equal) of the previous candle
             *   - the second candle does not close the window
             * The meaning of "near" and "equal" is specified with CandleSettings
             * outIntType is positive (100) or negative (-100):
             * it should be considered that upside or downside gap side-by-side white lines is significant when it appears in a trend,
             * while this function does not consider the trend
             */

            int outIdx = default;
            do
            {
                if (IsGapSideBySideWhiteLinesPattern(inOpen, inHigh, inLow, inClose, i, nearPeriodTotal, equalPeriodTotal))
                {
                    outIntType[outIdx++] = CandleHelpers.RealBodyGapUp(inOpen, inClose, i - 1, i - 2) ? 100 : -100;
                }
                else
                {
                    outIntType[outIdx++] = 0;
                }

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                nearPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearTrailingIdx - 1);

                equalPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, equalTrailingIdx - 1);

                i++;
                nearTrailingIdx++;
                equalTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsGapSideBySideWhiteLinesPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T nearPeriodTotal,
            T equalPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // upside or downside gap between the 1st candle and both the next 2 candles
            (
                CandleHelpers.RealBodyGapUp(inOpen, inClose, i - 1, i - 2) && CandleHelpers.RealBodyGapUp(inOpen, inClose, i, i - 2)
                ||
                CandleHelpers.RealBodyGapDown(inOpen, inClose, i - 1, i - 2) && CandleHelpers.RealBodyGapDown(inOpen, inClose, i, i - 2)
            )
            &&
            // 2nd: white
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.White &&
            // 3rd: white
            CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
            // same size 2 and 3
            CandleHelpers.RealBody(inClose, inOpen, i) >= CandleHelpers.RealBody(inClose, inOpen, i - 1) -
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal, i - 1) &&
            CandleHelpers.RealBody(inClose, inOpen, i) <= CandleHelpers.RealBody(inClose, inOpen, i - 1) +
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal, i - 1) &&
            // same open 2 and 3
            inOpen[i] >= inOpen[i - 1] -
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, equalPeriodTotal, i - 1) &&
            inOpen[i] <= inOpen[i - 1] +
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, equalPeriodTotal, i - 1);
    }

}
