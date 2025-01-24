using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode HighWave(
          ReadOnlySpan<float> inOpen,
          ReadOnlySpan<float> inHigh,
          ReadOnlySpan<float> inLow,
          ReadOnlySpan<float> inClose,
          Range inRange,
          Span<int> outIntType,
          out Range outRange)
        {
            return Candles.HighWave<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode HighWave<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            HighWaveImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int HighWaveLookback() =>
            Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryLong));

        
        
        

        private static Core.RetCode HighWave<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            HighWaveImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode HighWaveImpl<T>(
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

            var lookbackTotal = HighWaveLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyPeriodTotal = T.Zero;
            var bodyTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var shadowPeriodTotal = T.Zero;
            var shadowTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryLong);
            var i = bodyTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i);
                i++;
            }

            i = shadowTrailingIdx;
            while (i < startIdx)
            {
                shadowPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, i);
                i++;
            }

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - short real body
             *   - very long upper and lower shadow
             * The meaning of "short" and "very long" is specified with CandleSettings
             * outIntType is positive (100) when white or negative (-100) when black
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsHighWavePattern(inOpen, inHigh, inLow, inClose, i, bodyPeriodTotal, shadowPeriodTotal)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyTrailingIdx);

                shadowPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, shadowTrailingIdx);

                i++;
                bodyTrailingIdx++;
                shadowTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsHighWavePattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyPeriodTotal,
            T shadowPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // small real body
            CandleHelpers.RealBody(inClose, inOpen, i) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyPeriodTotal, i) &&
            // very long upper shadow
            CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, shadowPeriodTotal, i) &&
            // very long lower shadow
            CandleHelpers.LowerShadow(inClose, inOpen, inLow, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, shadowPeriodTotal, i);
    }

}
