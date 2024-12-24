using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {

        public static Core.RetCode Kama<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            KamaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int KamaLookback(int optInTimePeriod = 30) =>
            optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Kama);

        private static Core.RetCode Kama<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            KamaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode KamaImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
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

            var lookbackTotal = KamaLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            var sumROC1 = T.Zero;
            var today = startIdx - lookbackTotal;
            var trailingIdx = today;
            InitSumROC(inReal, ref sumROC1, ref today, optInTimePeriod);

            // At this point sumROC1 represent the summation of the 1-day price difference over the (optInTimePeriod - 1)

            // Calculate the first KAMA

            // The yesterday price is used here as the previous KAMA.
            var prevKAMA = inReal[today - 1];
            var tempReal = inReal[trailingIdx++];
            var periodROC = inReal[today] - tempReal;

            // Save the trailing value. Do this because input and output can point to the same buffer.
            var trailingValue = tempReal;

            var efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
            var smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

            // Calculate the KAMA like an EMA, using the smoothing constant as the adaptive factor.
            prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);

            // 'today' keep track of where the processing is within the input.
            while (today <= startIdx)
            {
                UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);
                periodROC = inReal[today] - inReal[trailingIdx - 1];
                efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
                smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

                // Calculate the KAMA like an EMA, using the smoothing constant as the adaptive factor.
                prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);
            }

            // Write the first value.
            outReal[0] = prevKAMA;
            var outIdx = 1;
            var outBegIdx = today - 1;

            // Skip the unstable period. Do the whole processing needed for KAMA, but do not write it in the output.
            while (today <= endIdx)
            {
                UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);
                periodROC = inReal[today] - inReal[trailingIdx - 1];
                efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
                smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

                // Calculate the KAMA like an EMA, using the smoothing constant as the adaptive factor.
                prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);

                outReal[outIdx++] = prevKAMA;
            }

            outRange = new Range(outBegIdx, outBegIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static void InitSumROC<T>(
            ReadOnlySpan<T> inReal,
            ref T sumROC1,
            ref int today,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            for (var i = optInTimePeriod; i > 0; i--)
            {
                var tempReal = inReal[today++];
                tempReal -= inReal[today];
                sumROC1 += T.Abs(tempReal);
            }
        }

        private static T CalcEfficiencyRatio<T>(T sumROC1, T periodROC) where T : IFloatingPointIeee754<T> =>
            sumROC1 <= periodROC || T.IsZero(sumROC1) ? T.One : T.Abs(periodROC / sumROC1);

        private static T CalcSmoothingConstant<T>(T efficiencyRatio) where T : IFloatingPointIeee754<T>
        {
            var constMax = FunctionHelpers.Two<T>() / (T.CreateChecked(30) + T.One);
            var constDiff = FunctionHelpers.Two<T>() / (FunctionHelpers.Two<T>() + T.One) - constMax;
            var tempReal = efficiencyRatio * constDiff + constMax;

            return tempReal * tempReal;
        }

        private static T CalcKAMA<T>(T todayValue, T prevKAMA, T smoothingConstant) where T : IFloatingPointIeee754<T> =>
            (todayValue - prevKAMA) * smoothingConstant + prevKAMA;

        private static void UpdateSumROC<T>(
            ReadOnlySpan<T> inReal,
            ref T sumROC1,
            ref int today,
            ref int trailingIdx,
            ref T trailingValue) where T : IFloatingPointIeee754<T>
        {
            var tempReal = inReal[today];
            var tempReal2 = inReal[trailingIdx++];

            /* Adjust sumROC1:
             *  - Remove trailing ROC1
             *  - Add new ROC1
             */
            sumROC1 -= T.Abs(trailingValue - tempReal2);
            sumROC1 += T.Abs(tempReal - inReal[today - 1]);

            // Save the trailing value. Do this because input and output can point to the same buffer.
            trailingValue = tempReal2;
        }
    }
}