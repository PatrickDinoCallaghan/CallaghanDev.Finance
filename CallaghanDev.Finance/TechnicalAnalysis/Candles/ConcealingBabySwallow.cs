
using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;
namespace CallaghanDev.Finance.TechnicalAnalysis;

public static partial class Candles
{
    public static Core.RetCode ConcealingBabySwallow(
      ReadOnlySpan<float> inOpen,
      ReadOnlySpan<float> inHigh,
      ReadOnlySpan<float> inLow,
      ReadOnlySpan<float> inClose,
      Range inRange,
      Span<int> outIntType,
      out Range outRange)
    {
        return Candles.ConcealingBabySwallow<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
    }
    public static Core.RetCode ConcealingBabySwallow<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<int> outIntType,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        ConcealingBabySwallowImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

    
    public static int ConcealingBabySwallowLookback() => CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort) + 3;

    
    
    
    
    private static Core.RetCode ConcealingBabySwallow<T>(
        T[] inOpen,
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        int[] outIntType,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        ConcealingBabySwallowImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

    private static Core.RetCode ConcealingBabySwallowImpl<T>(
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

        var lookbackTotal = ConcealingBabySwallowLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Do the calculation using tight loops.
        // Add-up the initial period, except for the last value.
        Span<T> shadowVeryShortPeriodTotal = new T[4];
        var shadowVeryShortTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.ShadowVeryShort);
        var i = shadowVeryShortTrailingIdx;
        while (i < startIdx)
        {
            shadowVeryShortPeriodTotal[3] +=
                CandleHelpers.CandleRange(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, i - 3);
            shadowVeryShortPeriodTotal[2] +=
                CandleHelpers.CandleRange(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, i - 2);
            shadowVeryShortPeriodTotal[1] +=
                CandleHelpers.CandleRange(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, i - 1);
            i++;
        }

        i = startIdx;

        /* Proceed with the calculation for the requested range.
         * Must have:
         *   - first candle: black marubozu (very short shadows)
         *   - second candle: black marubozu (very short shadows)
         *   - third candle: black candle that opens gapping down but has an upper shadow that extends into the prior body
         *   - fourth candle: black candle that completely engulfs the third candle, including the shadows
         * The meanings of "very short shadow" are specified with CandleSettings
         * outIntType is positive (100): concealing baby swallow is always bullish
         * it should be considered that concealing baby swallow is significant when it appears in downtrend,
         * while this function does not consider it
         */

        int outIdx = default;
        do
        {
            outIntType[outIdx++] = IsConcealingBabySwallowPattern(inOpen, inHigh, inLow, inClose, i, shadowVeryShortPeriodTotal)
                ? 100
                : 0;

            // add the current range and subtract the first range: this is done after the pattern recognition
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
            for (var totIdx = 3; totIdx >= 1; --totIdx)
            {
                shadowVeryShortPeriodTotal[totIdx] +=
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, i - totIdx) -
                    CandleHelpers.CandleRange(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort,
                        shadowVeryShortTrailingIdx - totIdx);
            }

            i++;
            shadowVeryShortTrailingIdx++;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static bool IsConcealingBabySwallowPattern<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        int i,
        Span<T> shadowVeryShortPeriodTotal) where T : IFloatingPointIeee754<T> =>
        // 1st black
        CandleHelpers.CandleColor(inClose, inOpen, i - 3) == Core.CandleColor.Black &&
        // 2nd black
        CandleHelpers.CandleColor(inClose, inOpen, i - 2) == Core.CandleColor.Black &&
        // 3rd black
        CandleHelpers.CandleColor(inClose, inOpen, i - 1) == Core.CandleColor.Black &&
        // 4th black
        CandleHelpers.CandleColor(inClose, inOpen, i) == Core.CandleColor.Black &&
        // 1st: marubozu
        CandleHelpers.LowerShadow(inClose, inOpen, inLow, i - 3) <
        CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[3],
            i - 3) &&
        CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i - 3) <
        CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[3],
            i - 3) &&
        // 2nd: marubozu
        CandleHelpers.LowerShadow(inClose, inOpen, inLow, i - 2) <
        CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[2],
            i - 2) &&
        CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i - 2) <
        CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[2],
            i - 2) &&
        // 3rd: opens gapping down
        CandleHelpers.RealBodyGapDown(inOpen, inClose, i - 1, i - 2) &&
        // and has an upper shadow
        CandleHelpers.UpperShadow(inHigh, inClose, inOpen, i - 1) >
        CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inOpen, Core.CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[1],
            i - 1) &&
        // that extends into the prior body
        inHigh[i - 1] > inClose[i - 2] &&
        // 4th: engulfs the 3rd including the shadows
        inHigh[i] > inHigh[i - 1] && inLow[i] < inLow[i - 1];
}
