using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode Tristar(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.Tristar<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode Tristar<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TristarImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int TristarLookback() => CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyDoji) + 2;

        
        
        

        private static Core.RetCode Tristar<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            TristarImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode TristarImpl<T>(
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

            var lookbackTotal = TristarLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyPeriodTotal = T.Zero;
            var bodyTrailingIdx = startIdx - 2 - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyDoji);
            var i = bodyTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, i);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - 3 consecutive doji days
             *   - the second doji is a star
             * The meaning of "doji" is specified with CandleSettings
             * outIntType is positive (100) when bullish or negative (-100) when bearish
             */

            int outIdx = default;
            do
            {
                if (IsTristarPattern(inOpen, inHigh, inLow, inClose, i, bodyPeriodTotal))
                {
                    // 3rd: doji
                    outIntType[outIdx] = 0;
                    if
                    (
                        // 2nd gaps up
                        CandleHelpers.RealBodyGapUp(inOpen, inClose, i - 1, i - 2) &&
                        // 3rd is not higher than 2nd
                        T.Max(inOpen[i], inClose[i]) < T.Max(inOpen[i - 1], inClose[i - 1])
                    )
                    {
                        outIntType[outIdx] = -100;
                    }

                    if
                    (
                        // 2nd gaps down
                        CandleHelpers.RealBodyGapDown(inOpen, inClose, i - 1, i - 2) &&
                        // 3rd is not lower than 2nd
                        T.Min(inOpen[i], inClose[i]) > T.Min(inOpen[i - 1], inClose[i - 1])
                    )
                    {
                        outIntType[outIdx] = 100;
                    }

                    outIdx++;
                }
                else
                {
                    outIntType[outIdx++] = 0;
                }

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, i - 2) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyTrailingIdx);

                i++;
                bodyTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsTristarPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // 1st: doji
            CandleHelpers.RealBody(inClose, inOpen, i - 2) <=
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyPeriodTotal, i - 2) &&
            // 2nd: doji
            CandleHelpers.RealBody(inClose, inOpen, i - 1) <=
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyPeriodTotal, i - 2) &&
            // 3rd: doji
            CandleHelpers.RealBody(inClose, inOpen, i) <=
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyDoji, bodyPeriodTotal, i - 2);
    }

}
