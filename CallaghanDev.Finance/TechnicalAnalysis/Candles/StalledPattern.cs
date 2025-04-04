using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {

        public static Core.RetCode StalledPattern(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.StalledPattern<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode StalledPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            StalledPatternImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int StalledPatternLookback() =>
            Math.Max(
                Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong),
                    CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort)),
                Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort),
                    CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near))
            ) + 2;

        
        
        

        private static Core.RetCode StalledPattern<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            StalledPatternImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode StalledPatternImpl<T>(
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

            var lookbackTotal = StalledPatternLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            Span<T> bodyLongPeriodTotal = new T[3];
            var bodyLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var bodyShortPeriodTotal = T.Zero;
            var bodyShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var shadowVeryShortPeriodTotal = T.Zero;
            var shadowVeryShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort);
            Span<T> nearPeriodTotal = new T[3];
            var nearTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near);
            var i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal[2] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 2);
                bodyLongPeriodTotal[1] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 1);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx)
            {
                bodyShortPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i);
                i++;
            }

            i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i - 1);
                i++;
            }

            i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal[2] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 2);
                nearPeriodTotal[1] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 1);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - three white candlesticks with consecutively higher closes
             *   - first candle: long white
             *   - second candle: long white with no or very short upper shadow opening within or near the previous white real body
             *     and closing higher than the prior candle
             *   - third candle: small white that gaps away or "rides on the shoulder" of the prior long real body
             *     (= it's at the upper end of the prior real body)
             * The meanings of "long", "very short", "short", "near" are specified with CandleSettings
             * outIntType is negative (-100): stalled pattern is always bearish
             * it should be considered that stalled pattern is significant when it appears in uptrend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsStalledPatternPattern(inOpen, inHigh, inLow, inClose, i, bodyLongPeriodTotal,
                    shadowVeryShortPeriodTotal, nearPeriodTotal, bodyShortPeriodTotal)
                    ? -100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                for (var totIdx = 2; totIdx >= 1; --totIdx)
                {
                    bodyLongPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong,
                            bodyLongTrailingIdx - totIdx);

                    nearPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearTrailingIdx - totIdx);
                }

                bodyShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyShortTrailingIdx);

                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort,
                        shadowVeryShortTrailingIdx - 1);

                i++;
                bodyLongTrailingIdx++;
                bodyShortTrailingIdx++;
                shadowVeryShortTrailingIdx++;
                nearTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsStalledPatternPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            Span<T> bodyLongPeriodTotal,
            T shadowVeryShortPeriodTotal,
            Span<T> nearPeriodTotal,
            T bodyShortPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // 1st white
            CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.White &&
            // 2nd white
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.White &&
            // 3rd white
            CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
            // consecutive higher closes
            inClose[i] > inClose[i - 1] && inClose[i - 1] > inClose[i - 2] &&
            // 1st: long real body
            CandleHelpers.RealBody(inClose, inOpen, i - 2) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal[2], i - 2) &&
            // 2nd: long real body
            CandleHelpers.RealBody(inClose, inOpen, i - 1) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal[1], i - 1) &&
            // very short upper shadow
            CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i - 1) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                i - 1) &&
            // opens within/near 1st real body
            inOpen[i - 1] > inOpen[i - 2] && inOpen[i - 1] <= inClose[i - 2] +
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal[2], i - 2) &&
            // 3rd: small real body
            CandleHelpers.RealBody(inClose, inOpen, i) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyShortPeriodTotal, i) &&
            // rides on the shoulder of 2nd real body
            inOpen[i] >= inClose[i - 1] - CandleHelpers.RealBody(inClose, inOpen, i) -
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal[1], i - 1);
    }

}
