using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode Mavp<T>(
            ReadOnlySpan<T> inReal,
            ReadOnlySpan<T> inPeriods,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInMinPeriod = 2,
            int optInMaxPeriod = 30,
            Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            MavpImpl(inReal, inPeriods, inRange, outReal, out outRange, optInMinPeriod, optInMaxPeriod, optInMAType);


        public static int MavpLookback(int optInMaxPeriod = 30, Core.MAType optInMAType = Core.MAType.Sma) =>
            optInMaxPeriod < 2 ? -1 : MaLookback(optInMaxPeriod, optInMAType);

        private static Core.RetCode Mavp<T>(
            T[] inReal,
            T[] inPeriods,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInMinPeriod = 2,
            int optInMaxPeriod = 30,
            Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            MavpImpl<T>(inReal, inPeriods, inRange, outReal, out outRange, optInMinPeriod, optInMaxPeriod, optInMAType);

        private static Core.RetCode MavpImpl<T>(
            ReadOnlySpan<T> inReal,
            ReadOnlySpan<T> inPeriods,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInMinPeriod,
            int optInMaxPeriod,
            Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length, inPeriods.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInMinPeriod < 2 || optInMaxPeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            var lookbackTotal = MavpLookback(optInMaxPeriod, optInMAType);
            if (inPeriods.Length < lookbackTotal)
            {
                return Core.RetCode.BadParam;
            }

            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            var tempInt = lookbackTotal > startIdx ? lookbackTotal : startIdx;
            if (tempInt > endIdx)
            {
                return Core.RetCode.Success;
            }

            var outputSize = endIdx - tempInt + 1;

            // Allocate intermediate local buffer.
            Span<T> localOutputArray = new T[outputSize];
            Span<int> localPeriodArray = new int[outputSize];

            // Copy caller array of period into local buffer. At the same time, truncate to min/max.
            for (var i = 0; i < outputSize; i++)
            {
                var period = Int32.CreateTruncating(inPeriods[startIdx + i]);
                localPeriodArray[i] = Math.Clamp(period, optInMinPeriod, optInMaxPeriod);
            }

            var intermediateOutput = outReal == inReal ? new T[outputSize] : outReal;

            /* Process each element of the input.
             * For each possible period value, the MA is calculated only once.
             * The outReal is then fill up for all element with the same period.
             * A local flag (value 0) is set in localPeriodArray to avoid doing a second time the same calculation.
             */
            var retCode = CalcMovingAverages(inReal, localPeriodArray, localOutputArray, new Range(startIdx, endIdx), outputSize, optInMAType,
                intermediateOutput);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            // Copy intermediate buffer to output buffer if necessary.
            if (intermediateOutput != outReal)
            {
                intermediateOutput[..outputSize].CopyTo(outReal);
            }

            outRange = new Range(startIdx, startIdx + outputSize);

            return Core.RetCode.Success;
        }

        private static Core.RetCode CalcMovingAverages<T>(
            ReadOnlySpan<T> real,
            Span<int> periodArray,
            Span<T> outputArray,
            Range range,
            int outputSize,
            Core.MAType maType,
            Span<T> intermediateOutput) where T : IFloatingPointIeee754<T>
        {
            for (var i = 0; i < outputSize; i++)
            {
                var curPeriod = periodArray[i];
                if (curPeriod == 0)
                {
                    continue;
                }

                // Calculation of the MA required.
                var retCode = MaImpl(real, range, outputArray, out _, curPeriod, maType);
                if (retCode != Core.RetCode.Success)
                {
                    return retCode;
                }

                intermediateOutput[i] = outputArray[i];
                for (var j = i + 1; j < outputSize; j++)
                {
                    if (periodArray[j] == curPeriod)
                    {
                        periodArray[j] = 0; // Flag to avoid recalculation
                        intermediateOutput[j] = outputArray[j];
                    }
                }
            }

            return Core.RetCode.Success;
        }
    }

}