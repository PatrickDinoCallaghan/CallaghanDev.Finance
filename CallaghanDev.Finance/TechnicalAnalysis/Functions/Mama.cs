using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode Mama<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMAMA,
            Span<T> outFAMA,
            out Range outRange,
            double optInFastLimit = 0.5,
            double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
            MamaImpl(inReal, inRange, outMAMA, outFAMA, out outRange, optInFastLimit, optInSlowLimit);


        public static int MamaLookback() =>
            /* The fix lookback is 32 and is established as follows:
             *
             * 12 price bar to be compatible with the implementation of TradeStation found in John Ehlers book.
             * 6 price bars for the Detrender
             * 6 price bars for Q1
             * 3 price bars for jI
             * 3 price bars for jQ
             * 1 price bar for Re/Im
             * 1 price bar for the Delta Phase
             * ────────
             * 32 Total
             */
            Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Mama) + 32;

        private static Core.RetCode Mama<T>(
            T[] inReal,
            Range inRange,
            T[] outMAMA,
            T[] outFAMA,
            out Range outRange,
            double optInFastLimit = 0.5,
            double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
            MamaImpl<T>(inReal, inRange, outMAMA, outFAMA, out outRange, optInFastLimit, optInSlowLimit);

        private static Core.RetCode MamaImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMAMA,
            Span<T> outFAMA,
            out Range outRange,
            double optInFastLimit,
            double optInSlowLimit) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInFastLimit < 0.01 || optInFastLimit > 0.99 || optInSlowLimit < 0.01 || optInSlowLimit > 0.99)
            {
                return Core.RetCode.BadParam;
            }

            var lookbackTotal = MamaLookback();
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            var outBegIdx = startIdx;

            FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
                out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

            int hilbertIdx = default;

            /* Initialize the circular buffer used by the hilbert transform logic.
             * A buffer is used for odd day and another for even days.
             * This minimizes the number of memory access and floating point operations needed
             * By using static circular buffer, no large dynamic memory allocation is needed for storing intermediate calculation.
             */
            Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

            int outIdx = default;

            T prevI2, prevQ2, re, im, mama, fama, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, prevPhase;
            var period = prevI2 = prevQ2
                = re = im = mama = fama = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = prevPhase = T.Zero;

            // The code is speed optimized and is most likely very hard to follow if you do not already know well the original algorithm.
            while (today <= endIdx)
            {
                var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

                var todayValue = inReal[today];
                FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, todayValue,
                    out var smoothedValue);

                var tempReal2 = PerformMAMAHilbertTransform(today, circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod,
                    ref i1ForOddPrev3, ref i1ForEvenPrev3, ref i1ForOddPrev2, ref i1ForEvenPrev2, prevQ2, prevI2, out var i2, out var q2);

                // Put Delta Phase into tempReal
                var tempReal = prevPhase - tempReal2;
                prevPhase = tempReal2;
                if (tempReal < T.One)
                {
                    tempReal = T.One;
                }

                // Put Alpha into tempReal
                if (tempReal > T.One)
                {
                    tempReal = T.CreateChecked(optInFastLimit) / tempReal;
                    if (tempReal < T.CreateChecked(optInSlowLimit))
                    {
                        tempReal = T.CreateChecked(optInSlowLimit);
                    }
                }
                else
                {
                    tempReal = T.CreateChecked(optInFastLimit);
                }

                // Calculate MAMA, FAMA
                mama = tempReal * todayValue + (T.One - tempReal) * mama;
                tempReal *= T.CreateChecked(0.5);
                fama = tempReal * mama + (T.One - tempReal) * fama;
                if (today >= startIdx)
                {
                    outMAMA[outIdx] = mama;
                    outFAMA[outIdx++] = fama;
                }

                // Adjust the period for next price bar
                FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

                today++;
            }

            outRange = new Range(outBegIdx, outBegIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static T PerformMAMAHilbertTransform<T>(
            int today,
            Span<T> circBuffer,
            T smoothedValue,
            ref int hilbertIdx,
            T adjustedPrevPeriod,
            ref T i1ForOddPrev3,
            ref T i1ForEvenPrev3,
            ref T i1ForOddPrev2,
            ref T i1ForEvenPrev2,
            T prevQ2,
            T prevI2,
            out T i2,
            out T q2) where T : IFloatingPointIeee754<T>
        {
            T tempReal2;
            if (today % 2 == 0)
            {
                FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                    prevI2,
                    out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);

                tempReal2 = !T.IsZero(i1ForEvenPrev3)
                    ? T.RadiansToDegrees(T.Atan(circBuffer[(int)FunctionHelpers.HTHelper.HilbertKeys.Q1] / i1ForEvenPrev3))
                    : T.Zero;
            }
            else
            {
                FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                    prevI2,
                    i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);

                tempReal2 = !T.IsZero(i1ForOddPrev3)
                    ? T.RadiansToDegrees(T.Atan(circBuffer[(int)FunctionHelpers.HTHelper.HilbertKeys.Q1] / i1ForOddPrev3))
                    : T.Zero;
            }

            return tempReal2;
        }
    }

}
