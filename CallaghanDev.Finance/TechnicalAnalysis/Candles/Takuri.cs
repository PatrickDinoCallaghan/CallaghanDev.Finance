using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {

        public static Core.RetCode Takuri(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.Takuri<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode Takuri<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TakuriImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int TakuriLookback() =>
            Math.Max(
                Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyDoji),
                    CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort)),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryLong));

        
        
        

        private static Core.RetCode Takuri<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TakuriImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode TakuriImpl<T>(
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

            var lookbackTotal = TakuriLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyDojiPeriodTotal = T.Zero;
            var bodyDojiTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyDoji);
            var shadowVeryShortPeriodTotal = T.Zero;
            var shadowVeryShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort);
            var shadowVeryLongPeriodTotal = T.Zero;
            var shadowVeryLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryLong);
            var i = bodyDojiTrailingIdx;
            while (i < startIdx)
            {
                bodyDojiPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, i);
                i++;
            }

            i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = shadowVeryLongTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, i);
                i++;
            }

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - doji body
             *   - open and close at the high of the day = no or very short upper shadow
             *   - very long lower shadow
             * The meaning of "doji", "very short" and "very long" is specified with CandleSettings
             * outIntType is always positive (100) but this does not mean it is bullish: takuri must be considered relatively to the trend
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsTakuriPattern(inOpen, inHigh, inLow, inClose, i, bodyDojiPeriodTotal, shadowVeryShortPeriodTotal,
                    shadowVeryLongPeriodTotal)
                    ? 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyDojiPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyDojiTrailingIdx);

                shadowVeryShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort,
                        shadowVeryShortTrailingIdx);

                shadowVeryLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, shadowVeryLongTrailingIdx);

                i++;
                bodyDojiTrailingIdx++;
                shadowVeryShortTrailingIdx++;
                shadowVeryLongTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsTakuriPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyDojiPeriodTotal,
            T shadowVeryShortPeriodTotal,
            T shadowVeryLongPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // doji boy
            CandleHelpers.RealBody(inClose, inOpen, i) <=
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyDojiPeriodTotal, i) &&
            // very short upper shadow
            CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal,
                i) &&
            // very long lower shadow
            CandleHelpers.LowerShadow(inClose, inOpen, inLow, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowVeryLong, shadowVeryLongPeriodTotal, i);
    }

}