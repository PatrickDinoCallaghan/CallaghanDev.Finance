using System.Numerics;
namespace TALib
{

    public static partial class Candles
    {
        
        public static Core.RetCode AbandonedBaby<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange,
            double optInPenetration = 0.3) where T : IFloatingPointIeee754<T> =>
            AbandonedBabyImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange, optInPenetration);

        
        public static int AbandonedBabyLookback() =>
            Math.Max(
                Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyDoji),
                    CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong)),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort)
            ) + 2;

        
        
        
        
        private static Core.RetCode AbandonedBaby<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange,
            double optInPenetration = 0.3) where T : IFloatingPointIeee754<T> =>
            AbandonedBabyImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange, optInPenetration);

        private static Core.RetCode AbandonedBabyImpl<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange,
            double optInPenetration) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inOpen.Length, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInPenetration < 0.0)
            {
                return Core.RetCode.BadParam;
            }

            var lookbackTotal = AbandonedBabyLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyLongPeriodTotal = T.Zero;
            var bodyDojiPeriodTotal = T.Zero;
            var bodyShortPeriodTotal = T.Zero;
            var bodyLongTrailingIdx = startIdx - 2 - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var bodyDojiTrailingIdx = startIdx - 1 - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyDoji);
            var bodyShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var i = bodyLongTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i);
                i++;
            }

            i = bodyDojiTrailingIdx;
            while (i < startIdx - 1)
            {
                bodyDojiPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, i);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx)
            {
                bodyShortPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - first candle: long white (black) real body
             *   - second candle: doji
             *   - third candle: black (white) real body that moves well within the first candle's real body
             *   - upside (downside) gap between the first candle and the doji (the shadows of the two candles don't touch)
             *   - downside (upside) gap between the doji and the third candle (the shadows of the two candles don't touch)
             * The meaning of "doji" and "long" is specified with CandleSettings
             * The meaning of "moves well within" is specified with optInPenetration and "moves" should mean
             * the real body should not be short ("short" is specified with CandleSettings) -
             * Greg Morris wants it to be long, someone else wants it to be relatively long
             * outIntType is positive (100) when it's an abandoned baby bottom or negative (-100) when it's an abandoned baby top
             * it should be considered that an abandoned baby is significant when it appears in an uptrend or downtrend,
             * while this function does not consider the trend
             */

            int outIdx = default;
            var penetration = T.CreateChecked(optInPenetration);
            do
            {
                outIntType[outIdx++] = IsAbandonedBabyPattern(inOpen, inHigh, inLow, inClose, i, bodyLongPeriodTotal, bodyDojiPeriodTotal,
                    bodyShortPeriodTotal, penetration)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 2) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx);

                bodyDojiPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, i - 1) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyDojiTrailingIdx);

                bodyShortPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyShortTrailingIdx);

                i++;
                bodyLongTrailingIdx++;
                bodyDojiTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsAbandonedBabyPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyLongPeriodTotal,
            T bodyDojiPeriodTotal,
            T bodyShortPeriodTotal,
            T penetration) where T : IFloatingPointIeee754<T> =>
            // 1st: long
            CandleHelpers.RealBody(inClose, inOpen, i - 2) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal, i - 2) &&
            // 2nd: doji
            CandleHelpers.RealBody(inClose, inOpen, i - 1) <=
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyDojiPeriodTotal, i - 1) &&
            // 3rd: longer than short
            CandleHelpers.RealBody(inClose, inOpen, i) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyShortPeriodTotal, i) &&
            (
                // 1st white
                CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.White &&
                // 3rd black
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
                // 3rd closes well within 1st real body
                inClose[i] < inClose[i - 2] - CandleHelpers.RealBody(inClose, inOpen, i - 2) * penetration &&
                // upside gap between 1st and 2nd
                CandleHelpers.CandleGapUp(inLow, inHigh, i - 1, i - 2) &&
                // downside gap between 2nd and 3rd
                CandleHelpers.CandleGapDown(inLow, inHigh, i, i - 1)
                ||
                // 1st black
                CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.Black &&
                // 3rd white
                CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
                // 3rd closes well within 1st real body
                inClose[i] > inClose[i - 2] + CandleHelpers.RealBody(inClose, inOpen, i - 2) * penetration &&
                // downside gap between 1st and 2nd
                CandleHelpers.CandleGapDown(inLow, inHigh, i - 1, i - 2) &&
                // upside gap between 2nd and 3rd
                CandleHelpers.CandleGapUp(inLow, inHigh, i, i - 1)
            );
    }

}
