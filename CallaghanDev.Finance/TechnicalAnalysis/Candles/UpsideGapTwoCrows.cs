using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode UpsideGapTwoCrows(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.UpsideGapTwoCrows<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode UpsideGapTwoCrows<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            UpsideGapTwoCrowsImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int UpsideGapTwoCrowsLookback() =>
            Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong)) + 2;

        private static Core.RetCode UpsideGapTwoCrows<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            UpsideGapTwoCrowsImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode UpsideGapTwoCrowsImpl<T>(
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

            var lookbackTotal = UpsideGapTwoCrowsLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyLongPeriodTotal = T.Zero;
            var bodyShortPeriodTotal = T.Zero;
            var bodyLongTrailingIdx = startIdx - 2 - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var bodyShortTrailingIdx = startIdx - 1 - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var i = bodyLongTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx - 1)
            {
                bodyShortPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - first candle: white candle, usually long
             *   - second candle: small black real body
             *   - gap between the first and the second candle's real bodies
             *   - third candle: black candle with a real body that engulfs the preceding candle
             *     and closes above the white candle's close
             * The meaning of "short" and "long" is specified with CandleSettings
             * outIntType is positive (100) when bullish or negative (-100) when bearish
             * it should be considered that an upside gap two crows is significant when it appears in an uptrend,
             * while this function does not consider the trend
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsUpsideGapTwoCrowsPattern(inOpen, inHigh, inLow, inClose, i, bodyLongPeriodTotal, bodyShortPeriodTotal)
                    ? -100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 2) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx);

                bodyShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyShortTrailingIdx);

                i++;
                bodyLongTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsUpsideGapTwoCrowsPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyLongPeriodTotal,
            T bodyShortPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // 1st: white
            CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.White &&
            // long body
            CandleHelpers.RealBody(inClose, inOpen, i - 2) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal, i - 2) &&
            // 2nd: black
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.Black &&
            // short
            CandleHelpers.RealBody(inClose, inOpen, i - 1) <=
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyShortPeriodTotal, i - 1) &&
            // gapping up
            CandleHelpers.RealBodyGapUp(inOpen, inClose, i - 1, i - 2) &&
            // 3rd: black
            CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
            // 3rd: engulfing prior rb
            inOpen[i] > inOpen[i - 1] && inClose[i] < inClose[i - 1] &&
            // closing above 1st
            inClose[i] > inClose[i - 2];
    }

}
