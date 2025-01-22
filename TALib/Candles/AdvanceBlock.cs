
using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Candles
    {

        public static Core.RetCode AdvanceBlock(
            ReadOnlySpan<float> inOpen,
            ReadOnlySpan<float> inHigh,
            ReadOnlySpan<float> inLow,
            ReadOnlySpan<float> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange)
        {
            return Candles.AdvanceBlock<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
        }
        public static Core.RetCode AdvanceBlock<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<int> outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AdvanceBlockImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        
        public static int AdvanceBlockLookback() =>
            Math.Max(
                Math.Max(
                    Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowLong),
                        CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowShort)),
                    Math.Max(CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Far),
                        CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near))),
                CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong)
            ) + 2;

        private static Core.RetCode AdvanceBlock<T>(
            T[] inOpen,
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            int[] outIntType,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AdvanceBlockImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

        private static Core.RetCode AdvanceBlockImpl<T>(
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

            var lookbackTotal = AdvanceBlockLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Do the calculation using tight loops.
            // Add-up the initial period, except for the last value.
            Span<T> shadowShortPeriodTotal = new T[3];
            var shadowShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowShort);
            Span<T> shadowLongPeriodTotal = new T[2];
            var shadowLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowLong);
            Span<T> nearPeriodTotal = new T[3];
            var nearTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Near);
            Span<T> farPeriodTotal = new T[3];
            var farTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.Far);
            var bodyLongPeriodTotal = T.Zero;
            var bodyLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
            var i = shadowShortTrailingIdx;
            while (i < startIdx)
            {
                shadowShortPeriodTotal[2] +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, i - 2);
                shadowShortPeriodTotal[1] +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, i - 1);
                shadowShortPeriodTotal[0] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, i);
                i++;
            }

            i = shadowLongTrailingIdx;
            while (i < startIdx)
            {
                shadowLongPeriodTotal[1] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, i - 1);
                shadowLongPeriodTotal[0] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, i);
                i++;
            }

            i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal[2] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 2);
                nearPeriodTotal[1] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - 1);
                i++;
            }

            i = farTrailingIdx;
            while (i < startIdx)
            {
                farPeriodTotal[2] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Far, i - 2);
                farPeriodTotal[1] += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Far, i - 1);
                i++;
            }

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 2);
                i++;
            }

            i = startIdx;

            /* Proceed with the calculation for the requested range.
             * Must have:
             *   - three white candlesticks with consecutively higher closes
             *   - each candle opens within or near the previous white real body
             *   - first candle: long white with no or very short upper shadow (a short shadow is accepted too for more flexibility)
             *   - second and third candles, or only third candle, show signs of weakening:
             *     progressively smaller white real bodies and/or relatively long upper shadows
             * The meanings of "long body", "short shadow", "far" and "near" are specified with CandleSettings
             * outIntType is negative (-100): advance block is always bearish
             * it should be considered that advance block is significant when it appears in uptrend,
             * while this function does not consider it
             */

            int outIdx = default;
            do
            {
                outIntType[outIdx++] = IsAdvanceBlockPattern(inOpen, inHigh, inLow, inClose, i, nearPeriodTotal, bodyLongPeriodTotal,
                    shadowShortPeriodTotal, farPeriodTotal, shadowLongPeriodTotal)
                    ? -100
                    : 0;

                // add the current range and subtract the first range: this is done after the pattern recognition
                // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                for (var totIdx = 2; totIdx >= 0; --totIdx)
                {
                    shadowShortPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort,
                            shadowShortTrailingIdx - totIdx);
                }

                for (var totIdx = 1; totIdx >= 0; --totIdx)
                {
                    shadowLongPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong,
                            shadowLongTrailingIdx - totIdx);
                }

                for (var totIdx = 2; totIdx >= 1; --totIdx)
                {
                    farPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Far, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Far, farTrailingIdx - totIdx);

                    nearPeriodTotal[totIdx] +=
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, i - totIdx) -
                        CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearTrailingIdx - totIdx);
                }

                bodyLongPeriodTotal +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 2) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx - 2);

                i++;
                shadowShortTrailingIdx++;
                shadowLongTrailingIdx++;
                nearTrailingIdx++;
                farTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool IsAdvanceBlockPattern<T>(
            ReadOnlySpan<T> inOpen,
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            int i,
            Span<T> nearPeriodTotal,
            T bodyLongPeriodTotal,
            Span<T> shadowShortPeriodTotal,
            Span<T> farPeriodTotal,
            Span<T> shadowLongPeriodTotal) where T : IFloatingPointIeee754<T> =>
            // 1st white
            CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.White &&
            // 2nd white
            CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.White &&
            // 3rd white
            CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.White &&
            // consecutive higher closes
            inClose[i] > inClose[i - 1] && inClose[i - 1] > inClose[i - 2] &&
            // 2nd opens within/near 1st real body
            inOpen[i - 1] > inOpen[i - 2] &&
            // 3rd opens within/near 2nd real body
            inOpen[i - 1] <= inClose[i - 2] +
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal[2], i - 2) &&
            inOpen[i] > inOpen[i - 1] &&
            inOpen[i] <= inClose[i - 1] +
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal[1], i - 1) &&
            // 1st: long real body
            CandleHelpers.RealBody(inClose, inOpen, i - 2) >
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal, i - 2) &&
            // 1st: short upper shadow
            CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i - 2) <
            CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, shadowShortPeriodTotal[2], i - 2) &&
            (
                // (2 far smaller than 1 && 3 not longer than 2)
                // advance blocked with the 2nd, 3rd must not carry on the advance
                CandleHelpers.RealBody(inClose, inOpen, i - 1) < CandleHelpers.RealBody(inClose, inOpen, i - 2) -
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Far, farPeriodTotal[2], i - 2) &&
                CandleHelpers.RealBody(inClose, inOpen, i) < CandleHelpers.RealBody(inClose, inOpen, i - 1) +
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Near, nearPeriodTotal[1], i - 1)
                ||
                // 3 far smaller than 2
                // advance blocked with the 3rd
                CandleHelpers.RealBody(inClose, inOpen, i) < CandleHelpers.RealBody(inClose, inOpen, i - 1) -
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.Far, farPeriodTotal[1], i - 1)
                ||
                // (3 smaller than 2 && 2 smaller than 1 && (3 or 2 not short upper shadow))
                // advance blocked with progressively smaller real bodies and some upper shadows
                CandleHelpers.RealBody(inClose, inOpen, i) < CandleHelpers.RealBody(inClose, inOpen, i - 1) &&
                CandleHelpers.RealBody(inClose, inOpen, i - 1) < CandleHelpers.RealBody(inClose, inOpen, i - 2) &&
                (
                    CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) >
                    CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, shadowShortPeriodTotal[0],
                        i)
                    ||
                    CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i - 1) >
                    CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowShort, shadowShortPeriodTotal[1],
                        i - 1)
                )
                ||
                // (3 smaller than 2 && 3 long upper shadow)
                // advance blocked with 3rd candle's long upper shadow and smaller body
                CandleHelpers.RealBody(inClose, inOpen, i) < CandleHelpers.RealBody(inClose, inOpen, i - 1) &&
                CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i) >
                CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.ShadowLong, shadowLongPeriodTotal[0], i)
            );
    }
}
