
using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {

        public static Core.RetCode BeltHold(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.BeltHold<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode BeltHold<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            BeltHoldImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        
        public static int BeltHoldLookback() =>
            Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort));

        
        
        
        
        private static Core.RetCode BeltHold<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            BeltHoldImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode BeltHoldImpl<T>(
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

            var lookbackTotal = BeltHoldLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyLongPeriodTotal = T.Zero;
            var bodyLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var shadowVeryShortPeriodTotal = T.Zero;
            var shadowVeryShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort);
            var i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i);
                i++;
            }

            i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - long white (black) real body
             *   - no or very short lower (upper) shadow
             * The meaning of "long" and "very short" is specified with CandleSettings
             * outIntType is positive (100) when white (bullish), negative (-100) when black (bearish)
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsBeltHoldPattern(inOpen, inHigh, inLow, inClose, i, bodyLongPeriodTotal, shadowVeryShortPeriodTotal)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx);

                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort,
                        shadowVeryShortTrailingIdx);

                i++;
                bodyLongTrailingIdx++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsBeltHoldPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyLongPeriodTotal,
            T shadowVeryShortPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // long body
            CandleHelpers.RealBody(inClose, inOpen, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal, i) &&
            (
                // white body and very short lower shadow
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
                CandleHelpers.LowerShadow(inClose, inOpen, inLow, i) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                    i)
                ||
                // black body and very short upper shadow
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
                CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                    i)
            );
    }

}