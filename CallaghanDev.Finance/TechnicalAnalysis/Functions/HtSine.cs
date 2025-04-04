using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode HtSine<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outSine,
            Span<T> outLeadSine,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            HtSineImpl(inReal, inRange, outSine, outLeadSine, out outRange);

        public static int HtSineLookback() =>
            /*  31 input are skip
             * +32 output are skip to account for misc lookback
             * ──────────────────
             *  63 Total Lookback
             *
             * See MamaLookback for an explanation of the "32"
             */
            Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtSine) + 63;

        private static Core.RetCode HtSine<T>(
            T[] inReal,
            Range inRange,
            T[] outSine,
            T[] outLeadSine,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            HtSineImpl<T>(inReal, inRange, outSine, outLeadSine, out outRange);

        private static Core.RetCode HtSineImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outSine,
            Span<T> outLeadSine,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            var lookbackTotal = HtSineLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            const int smoothPriceSize = 50;
            Span<T> smoothPrice = new T[smoothPriceSize];

            var outBegIdx = startIdx;

            FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
                out var trailingWMAValue, out var trailingWMAIdx, 34, out var today);

            int hilbertIdx = default;
            int smoothPriceIdx = default;

            /* Initialize the circular buffer used by the hilbert transform logic.
             * A buffer is used for odd day and another for even days.
             * This minimizes the number of memory access and floating point operations needed
             * By using static circular buffer, no large dynamic memory allocation is needed for storing intermediate calculation.
             */
            Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

            int outIdx = default;

            T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
            var period = prevI2 = prevQ2 =
                re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = T.Zero;

            // The code is speed optimized and is most likely very hard to follow if you do not already know well the original algorithm.
            while (today <= endIdx)
            {
                var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

                FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                    out var smoothedValue);

                // Remember the smoothedValue into the smoothPrice circular buffer.
                smoothPrice[smoothPriceIdx] = smoothedValue;

                PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                    ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

                // Adjust the period for next price bar
                FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

                smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

                dcPhase = ComputeDcPhase(smoothPrice, smoothPeriod, smoothPriceIdx, dcPhase);

                if (today >= startIdx)
                {
                    outSine[outIdx] = T.Sin(T.DegreesToRadians(dcPhase));
                    outLeadSine[outIdx++] = T.Sin(T.DegreesToRadians(dcPhase + T.CreateChecked(45)));
                }

                if (++smoothPriceIdx > smoothPriceSize - 1)
                {
                    smoothPriceIdx = 0;
                }

                today++;
            }

            outRange = new Range(outBegIdx, outBegIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}