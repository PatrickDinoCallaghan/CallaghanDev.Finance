using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode InvertedHammer(
          ReadOnlySpan<float> inOpen,
          ReadOnlySpan<float> inHigh,
          ReadOnlySpan<float> inLow,
          ReadOnlySpan<float> inClose,
          Range inRange,
          Span<int> outIntType,
          out Range outRange)
        {
            return Candles.InvertedHammer<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode InvertedHammer<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            InvertedHammerImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int InvertedHammerLookback() =>
            Math.Max(
                Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort),
                    CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowLong)),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort)
            ) + 1;

        private static Core.RetCode InvertedHammer<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            InvertedHammerImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode InvertedHammerImpl<T>(
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

            var lookbackTotal = InvertedHammerLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyPeriodTotal = T.Zero;
            var bodyTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var shadowLongPeriodTotal = T.Zero;
            var shadowLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowLong);
            var shadowVeryShortPeriodTotal = T.Zero;
            var shadowVeryShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort);
            var i = bodyTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i);
                i++;
            }

            i = shadowLongTrailingIdx;
            while (i < startIdx)
            {
                shadowLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, i);
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
             *   - small real body
             *   - long upper shadow
             *   - no, or very short, lower shadow
             *   - gap down
             * The meaning of "short", "very short" and "long" is specified with CandleSettings
             * outIntType is positive (100): inverted hammer is always bullish
             * it should be considered that an inverted hammer must appear in a downtrend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsInvertedHammerPattern(inOpen, inHigh, inLow, inClose, i, bodyPeriodTotal, shadowLongPeriodTotal,
                    shadowVeryShortPeriodTotal)
                    ? 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyTrailingIdx);

                shadowLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, shadowLongTrailingIdx);

                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort,
                        shadowVeryShortTrailingIdx);

                i++;
                bodyTrailingIdx++;
                shadowLongTrailingIdx++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsInvertedHammerPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyPeriodTotal,
            T shadowLongPeriodTotal,
            T shadowVeryShortPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // small real body
            CandleHelpers.RealBody(inClose, inOpen, i) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyPeriodTotal, i) &&
            // long upper shadow
            CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, shadowLongPeriodTotal, i) &&
            // very short lower shadow
            CandleHelpers.LowerShadow(inClose, inOpen, inLow, i) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                i) &&
            // gap down
            CandleHelpers.RealBodyGapDown(inOpen, inClose, i, i - 1);
    }

}
