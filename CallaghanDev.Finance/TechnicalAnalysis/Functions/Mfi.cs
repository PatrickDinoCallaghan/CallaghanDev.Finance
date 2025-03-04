using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis;

public static partial class Functions
{
    
    public static Core.RetCode Mfi<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MfiImpl(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInTimePeriod);

    
    public static int MfiLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Mfi);

    private static Core.RetCode Mfi<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        T[] inVolume,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MfiImpl<T>(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MfiImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = MfiLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        int outIdx = default;

        var moneyFlow = new (T negative, T positive)[optInTimePeriod];

        int mflowIdx = default;
        var maxIdxMflow = optInTimePeriod - 1;

        // Accumulate the positive and negative money flow among the initial period.
        var today = startIdx - lookbackTotal;
        var prevValue = (inHigh[today] + inLow[today] + inClose[today]) / FunctionHelpers.Three<T>();

        var posSumMF = T.Zero;
        var negSumMF = T.Zero;
        today++;
        AccumulateInitialMoneyFlow(inHigh, inLow, inClose, inVolume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
            ref mflowIdx, maxIdxMflow, optInTimePeriod);

        /* The following two equations are equivalent:
         *   MFI = 100 - (100 / 1 + (posSumMF / negSumMF))
         *   MFI = 100 * (posSumMF / (posSumMF + negSumMF))
         * The second equation is used here for speed optimization.
         */
        if (today > startIdx)
        {
            var tempValue1 = posSumMF + negSumMF;
            outReal[outIdx++] = tempValue1 >= T.One ? FunctionHelpers.Hundred<T>() * (posSumMF / tempValue1) : T.Zero;
        }
        else
        {
            // Skip the unstable period. Do the processing but do not write it in the output.
            today = SkipMfiUnstablePeriod(inHigh, inLow, inClose, inVolume, today, startIdx, moneyFlow, maxIdxMflow, ref posSumMF,
                ref mflowIdx, ref negSumMF, ref prevValue);
        }

        while (today <= endIdx)
        {
            posSumMF -= moneyFlow[mflowIdx].positive;
            negSumMF -= moneyFlow[mflowIdx].negative;

            UpdateMoneyFlow(inHigh, inLow, inClose, inVolume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
                ref mflowIdx);

            var tempValue1 = posSumMF + negSumMF;
            outReal[outIdx++] = tempValue1 >= T.One ? FunctionHelpers.Hundred<T>() * (posSumMF / tempValue1) : T.Zero;

            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void AccumulateInitialMoneyFlow<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        ref int today,
        ref T prevValue,
        ref T posSumMF,
        ref T negSumMF,
        (T negative, T positive)[] moneyFlow,
        ref int mflowIdx,
        int maxIdxMflow,
        int timePeriod) where T : IFloatingPointIeee754<T>
    {
        for (var i = timePeriod; i > 0; i--)
        {
            UpdateMoneyFlow(high, low, close, volume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
                ref mflowIdx);

            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }
    }

    private static int SkipMfiUnstablePeriod<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        int today,
        int startIdx,
        (T negative, T positive)[] moneyFlow,
        int maxIdxMflow,
        ref T posSumMF,
        ref int mflowIdx,
        ref T negSumMF,
        ref T prevValue) where T : IFloatingPointIeee754<T>
    {
        while (today < startIdx)
        {
            posSumMF -= moneyFlow[mflowIdx].positive;
            negSumMF -= moneyFlow[mflowIdx].negative;

            UpdateMoneyFlow(high, low, close, volume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
                ref mflowIdx);

            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }

        return today;
    }

    private static void UpdateMoneyFlow<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        ref int today,
        ref T prevValue,
        ref T posSumMF,
        ref T negSumMF,
        (T negative, T positive)[] moneyFlow,
        ref int mflowIdx) where T : IFloatingPointIeee754<T>
    {
        var tempValue1 = (high[today] + low[today] + close[today]) / FunctionHelpers.Three<T>();
        var tempValue2 = tempValue1 - prevValue;
        prevValue = tempValue1;
        tempValue1 *= volume[today++];
        if (tempValue2 < T.Zero)
        {
            moneyFlow[mflowIdx].negative = tempValue1;
            negSumMF += tempValue1;
            moneyFlow[mflowIdx].positive = T.Zero;
        }
        else if (tempValue2 > T.Zero)
        {
            moneyFlow[mflowIdx].positive = tempValue1;
            posSumMF += tempValue1;
            moneyFlow[mflowIdx].negative = T.Zero;
        }
        else
        {
            moneyFlow[mflowIdx].positive = T.Zero;
            moneyFlow[mflowIdx].negative = T.Zero;
        }
    }
}
