using System.Numerics;

namespace TALib;

public static partial class Functions
{
    
    public static Core.RetCode MinMax<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMin,
        Span<T> outMax,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxImpl(inReal, inRange, outMin, outMax, out outRange, optInTimePeriod);

    
    public static int MinMaxLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    
    
    
    
    private static Core.RetCode MinMax<T>(
        T[] inReal,
        Range inRange,
        T[] outMin,
        T[] outMax,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxImpl<T>(inReal, inRange, outMin, outMax, out outRange, optInTimePeriod);

    private static Core.RetCode MinMaxImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMin,
        Span<T> outMax,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = MinMaxLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Proceed with the calculation for the requested range.
        // The algorithm allows the input and output to be the same buffer.
        int outIdx = default;
        var today = startIdx;
        var trailingIdx = startIdx - lookbackTotal;

        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;
        while (today <= endIdx)
        {
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

            outMax[outIdx] = highest;
            outMin[outIdx++] = lowest;
            trailingIdx++;
            today++;
        }

        // Keep the outBegIdx relative to the caller input before returning.
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
