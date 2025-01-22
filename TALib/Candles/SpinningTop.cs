using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Candles
    {
        public static Core.RetCode SpinningTop(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.SpinningTop<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode SpinningTop<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            SpinningTopImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);


        public static int SpinningTopLookback() => CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);

        
        
        

        private static Core.RetCode SpinningTop<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            SpinningTopImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode SpinningTopImpl<T>(
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

            var lookbackTotal = SpinningTopLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            var bodyPeriodTotal = T.Zero;
            var bodyTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyShort);
            var i = bodyTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i);
                i++;
            }

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - small real body
             *   - shadows longer than the real body
             * The meaning of "short" is specified with CandleSettings
             * outIntType is positive (100) when white or negative (-100) when black
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsSpinningTopPattern(inOpen, inHigh, inLow, inClose, i, bodyPeriodTotal)
                    ? (int)CandleHelpers.CandleColor(inClose, inOpen, i) * 100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                bodyPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, i) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyTrailingIdx);

                i++;
                bodyTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsSpinningTopPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            T bodyPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // short body
            CandleHelpers.RealBody(inClose, inOpen, i) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyShort, bodyPeriodTotal, i) &&
            // upper shadow longer real body
            CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) > CandleHelpers.RealBody(inClose, inOpen, i) &&
            // lower shadow longer real body
            CandleHelpers.LowerShadow(inClose, inOpen, inLow, i) > CandleHelpers.RealBody(inClose, inOpen, i);
    }

}
