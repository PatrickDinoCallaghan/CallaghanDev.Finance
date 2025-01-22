
using System.Numerics;
using TALib.Helpers;
namespace TALib;

public static partial class Candles
{
    public static Core.RetCode Breakaway(
        ReadOnlySpan<float> inOpen,
        ReadOnlySpan<float> inHigh,
        ReadOnlySpan<float> inLow,
        ReadOnlySpan<float> inClose,
        Range inRange,
        Span<int> outIntType,
        out Range outRange)
    {
        return Candles.Breakaway<float>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);
    }
    public static Core.RetCode Breakaway<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<int> outIntType,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        BreakawayImpl(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

    
    public static int BreakawayLookback() => CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong) + 4;

    private static Core.RetCode Breakaway<T>(
        T[] inOpen,
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        int[] outIntType,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        BreakawayImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outIntType, out outRange);

    private static Core.RetCode BreakawayImpl<T>(
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

        var lookbackTotal = BreakawayLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Do the calculation using tight loops.
        // Add-up the initial period, except for the last value.
        var bodyLongPeriodTotal = T.Zero;
        var bodyLongTrailingIdx = startIdx - CandleHelpers.CandleAveragePeriod(Core.CandleSettingType.BodyLong);
        var i = bodyLongTrailingIdx;
        while (i < startIdx)
        {
            bodyLongPeriodTotal += CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 4);
            i++;
        }

        i = startIdx;

        /* Proceed with the calculation for the requested range.
         * Must have:
         *   - first candle: long black (white)
         *   - second candle: black (white) day whose body gaps down (up)
         *   - third candle: black or white day with lower (higher) high and lower (higher) low than prior candle's
         *   - fourth candle: black (white) day with lower (higher) high and lower (higher) low than prior candle's
         *   - fifth candle: white (black) day that closes inside the gap, erasing the prior 3 days
         * The meaning of "long" is specified with CandleSettings
         * outIntType is positive (100) when bullish or negative (-100) when bearish
         * it should be considered that breakaway is significant in a trend opposite to the last candle,
         * while this function does not consider it
         */

        int outIdx = default;
        do
        {
            outIntType[outIdx++] = IsBreakawayPattern(inOpen, inHigh, inLow, inClose, i, bodyLongPeriodTotal)
                ? (int) CandleHelpers.CandleColor(inClose, inOpen, i) * 100
                : 0;

            // add the current range and subtract the first range: this is done after the pattern recognition
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
            bodyLongPeriodTotal +=
                CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, i - 4) -
                CandleHelpers.CandleRange(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongTrailingIdx - 4);

            i++;
            bodyLongTrailingIdx++;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static bool IsBreakawayPattern<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        int i,
        T bodyLongPeriodTotal) where T : IFloatingPointIeee754<T> =>
        // 1st long
        CandleHelpers.RealBody(inClose, inOpen, i - 4) >
        CandleHelpers.CandleAverage(inOpen, inHigh, inLow, inClose, Core.CandleSettingType.BodyLong, bodyLongPeriodTotal, i - 4) &&
        // 1st, 2nd, 4th same color, 5th opposite
        CandleHelpers.CandleColor(inClose, inOpen, i - 4) == CandleHelpers.CandleColor(inClose, inOpen, i - 3) &&
        CandleHelpers.CandleColor(inClose, inOpen, i - 3) == CandleHelpers.CandleColor(inClose, inOpen, i - 1) &&
        (int) CandleHelpers.CandleColor(inClose, inOpen, i - 1) == -(int) CandleHelpers.CandleColor(inClose, inOpen, i) &&
        (
            // when 1st is black:
            CandleHelpers.CandleColor(inClose, inOpen, i - 4) == Core.CandleColor.Black &&
            // 2nd gaps down
            CandleHelpers.RealBodyGapDown(inOpen, inClose, i - 3, i - 4) &&
            // 3rd has lower high and low than 2nd
            inHigh[i - 2] < inHigh[i - 3] && inLow[i - 2] < inLow[i - 3] &&
            // 4th has lower high and low than 3rd
            inHigh[i - 1] < inHigh[i - 2] && inLow[i - 1] < inLow[i - 2] &&
            // 5th closes inside the gap
            inClose[i] > inOpen[i - 3] && inClose[i] < inClose[i - 4]
            ||
            // when 1st is white:
            CandleHelpers.CandleColor(inClose, inOpen, i - 4) == Core.CandleColor.White &&
            // 2nd gaps up
            CandleHelpers.RealBodyGapUp(inClose, inOpen, i - 3, i - 4) &&
            // 3rd has higher high and low than 2nd
            inHigh[i - 2] > inHigh[i - 3] && inLow[i - 2] > inLow[i - 3] &&
            // 4th has higher high and low than 3rd
            inHigh[i - 1] > inHigh[i - 2] && inLow[i - 1] > inLow[i - 2] &&
            // 5th closes inside the gap
            inClose[i] < inOpen[i - 3] && inClose[i] > inClose[i - 4]
        );
}
