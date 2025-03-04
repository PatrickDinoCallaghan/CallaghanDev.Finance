using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode SeparatingLines(
          ReadOnlySpan<float> inOpen,
          ReadOnlySpan<float> inHigh,
          ReadOnlySpan<float> inLow,
          ReadOnlySpan<float> inClose,
          Range inRange,
          Span<int> outIntType,
          out Range outRange)
        {
            return Candles.SeparatingLines<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode SeparatingLines<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            SeparatingLinesImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int SeparatingLinesLookback() =>
            Math.Max(
                Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort),
                    CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong)),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Equal)
            ) + 1;

        
        
        

        private static Core.RetCode SeparatingLines<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            SeparatingLinesImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode SeparatingLinesImpl<T>(
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

            var lookbackTotal = SeparatingLinesLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var shadowVeryShortPeriodTotal = T.Zero;
            var shadowVeryShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort);
            var bodyLongPeriodTotal = T.Zero;
            var bodyLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var equalPeriodTotal = T.Zero;
            var equalTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Equal);
            var i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i);
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
             *   - first candle: black (white) candle
             *   - second candle: bullish (bearish) belt hold with the same open as the prior candle
             * The meaning of "long body" and "very short shadow" of the belt hold is specified with CandleSettings
             * outIntType is positive (100) when bullish or negative (-100) when bearish
             * it should be considered that separating lines is significant when coming in a trend and
             * the belt hold has the same direction of the trend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsSeparatingLinesPattern(inOpen, inHigh, inLow, inClose, i, equalPeriodTotal, bodyLongPeriodTotal,
                    shadowVeryShortPeriodTotal)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort,
                        shadowVeryShortTrailingIdx);

                bodyLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx);

                equalPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, equalTrailingIdx - 1);

                i++;
                shadowVeryShortTrailingIdx++;
                bodyLongTrailingIdx++;
                equalTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsSeparatingLinesPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T equalPeriodTotal,
            T bodyLongPeriodTotal,
            T shadowVeryShortPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // opposite candles
            (int)CandleHelpers.CandleColor(inClose, inOpen, i - 1) == -(int)CandleHelpers.CandleColor(inClose, inOpen, i) &&
            // same open
            inOpen[i] <= inOpen[i - 1] +
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, equalPeriodTotal, i - 1) &&
            inOpen[i] >= inOpen[i - 1] -
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Equal, equalPeriodTotal, i - 1) &&
            // belt hold: long body
            CandleHelpers.RealBody(inClose, inOpen, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal, i) &&
            (
                // with no lower shadow if bullish
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
                CandleHelpers.LowerShadow(inClose, inOpen, inLow, i) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                    i)
                ||
                // with no upper shadow if bearish
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
                CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                    i)
            );
    }

}
