using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Candles
    {
        public static Core.RetCode RisingFallingThreeMethods(
          ReadOnlySpan<float> inOpen,
          ReadOnlySpan<float> inHigh,
          ReadOnlySpan<float> inLow,
          ReadOnlySpan<float> inClose,
          Range inRange,
          Span<int> outIntType,
          out Range outRange)
        {
            return Candles.RisingFallingThreeMethods<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode RisingFallingThreeMethods<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            RisingFallingThreeMethodsImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int RisingFallingThreeMethodsLookback() =>
            Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong)) + 4;

        
        
        

        private static Core.RetCode RisingFallingThreeMethods<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            RisingFallingThreeMethodsImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode RisingFallingThreeMethodsImpl<T>(
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

            var lookbackTotal = RisingFallingThreeMethodsLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            Span<T> bodyPeriodTotal = new T[5];
            var bodyShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var bodyLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var i = bodyShortTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal[3] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i - 3);
                bodyPeriodTotal[2] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i - 2);
                bodyPeriodTotal[1] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i - 1);
                i++;
            }

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal[4] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 4);
                bodyPeriodTotal[0] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - first candle: long white (black) candlestick
             *   - then: group of falling (rising) small real body candlesticks (commonly black (white)) that hold within
             *     the prior long candle's range: ideally they should be three but two or more than three are ok too
             *   - final candle: long white (black) candle that opens above (below) the previous small candle's close
             *     and closes above (below) the first long candle's close
             * The meaning of "short" and "long" is specified with CandleSettings
             * here only patterns with 3 small candles are considered
             * outIntType is positive (100) or negative (-100)
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsRisingFallingThreeMethodsPattern(inOpen, inHigh, inLow, inClose, i, bodyPeriodTotal)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i - 4) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyPeriodTotal[4] +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 4) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx - 4);

                for (var totIdx = 3; totIdx >= 1; --totIdx)
                {
                    bodyPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort,
                            bodyShortTrailingIdx - totIdx);
                }

                bodyPeriodTotal[0] +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx);

                i++;
                bodyShortTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsRisingFallingThreeMethodsPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            Span<T> bodyPeriodTotal) where T : IFloatingPointIeee754<T>
        {
            var fifthCandleColor = T.CreateChecked((int)CandleHelpers.CandleColor(inClose, inOpen, i - 4) * 100);

            return
                // 1st long, then 3 small, 5th long
                CandleHelpers.RealBody(inClose, inOpen, i - 4) >
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyPeriodTotal[4], i - 4) &&
                CandleHelpers.RealBody(inClose, inOpen, i - 3) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyPeriodTotal[3], i - 3) &&
                CandleHelpers.RealBody(inClose, inOpen, i - 2) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyPeriodTotal[2], i - 2) &&
                CandleHelpers.RealBody(inClose, inOpen, i - 1) <
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyPeriodTotal[1], i - 1) &&
                CandleHelpers.RealBody(inClose, inOpen, i) >
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyPeriodTotal[0], i) &&
                // white, 3 black, white or black, 3 white, black
                (int)CandleHelpers.CandleColor(inClose, inOpen, i - 4) == -(int)CandleHelpers.CandleColor(inClose, inOpen, i - 3) &&
                CandleHelpers.CandleColor(inClose, inOpen, i - 3) == CandleHelpers.CandleColor(inClose, inOpen, i - 2) &&
                CandleHelpers.CandleColor(inClose, inOpen, i - 2) == CandleHelpers.CandleColor(inClose, inOpen, i - 1) &&
                (int)CandleHelpers.CandleColor(inClose, inOpen, i - 1) == -(int)CandleHelpers.CandleColor(inClose, inOpen, i) &&
                // 2nd to 4th hold within 1st: a part of the real body must be within 1st range
                T.Min(inOpen[i - 3], inClose[i - 3]) < inHigh[i - 4] && T.Max(inOpen[i - 3], inClose[i - 3]) > inLow[i - 4] &&
                T.Min(inOpen[i - 2], inClose[i - 2]) < inHigh[i - 4] && T.Max(inOpen[i - 2], inClose[i - 2]) > inLow[i - 4] &&
                T.Min(inOpen[i - 1], inClose[i - 1]) < inHigh[i - 4] && T.Max(inOpen[i - 1], inClose[i - 1]) > inLow[i - 4] &&
                // 2nd to 4th are falling (rising)
                inClose[i - 2] * fifthCandleColor < inClose[i - 3] * fifthCandleColor &&
                inClose[i - 1] * fifthCandleColor < inClose[i - 2] * fifthCandleColor &&
                // 5th opens above (below) the prior close
                inOpen[i] * fifthCandleColor > inClose[i - 1] * fifthCandleColor &&
                // 5th closes above (below) the 1st close
                inClose[i] * fifthCandleColor > inClose[i - 4] * fifthCandleColor;
        }
    }
}