
using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode MacdExt<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMACD,
            Span<T> outMACDSignal,
            Span<T> outMACDHist,
            out Range outRange,
            int optInFastPeriod = 12,
            Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26,
            Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9,
            Core.MAType optInSignalMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            MacdExtImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInFastMAType, optInSlowPeriod,
                optInSlowMAType, optInSignalPeriod, optInSignalMAType);


        public static int MacdExtLookback(
            int optInFastPeriod = 12,
            Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26,
            Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9,
            Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
            {
                return -1;
            }

            var lookbackLargest = MaLookback(optInFastPeriod, optInFastMAType);
            var tempInteger = MaLookback(optInSlowPeriod, optInSlowMAType);
            if (tempInteger > lookbackLargest)
            {
                lookbackLargest = tempInteger;
            }

            return lookbackLargest + MaLookback(optInSignalPeriod, optInSignalMAType);
        }





        private static Core.RetCode MacdExt<T>(
            T[] inReal,
            Range inRange,
            T[] outMACD,
            T[] outMACDSignal,
            T[] outMACDHist,
            out Range outRange,
            int optInFastPeriod = 12,
            Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26,
            Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9,
            Core.MAType optInSignalMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            MacdExtImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInFastMAType,
                optInSlowPeriod, optInSlowMAType, optInSignalPeriod, optInSignalMAType);

        private static Core.RetCode MacdExtImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMACD,
            Span<T> outMACDSignal,
            Span<T> outMACDHist,
            out Range outRange,
            int optInFastPeriod,
            Core.MAType optInFastMAType,
            int optInSlowPeriod,
            Core.MAType optInSlowMAType,
            int optInSignalPeriod,
            Core.MAType optInSignalMAType) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            // Make sure slow is really slower than the fast period. if not, swap.
            if (optInSlowPeriod < optInFastPeriod)
            {
                (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
                (optInSlowMAType, optInFastMAType) = (optInFastMAType, optInSlowMAType);
            }

            // Add the lookback needed for the signal line
            var lookbackSignal = MaLookback(optInSignalPeriod, optInSignalMAType);
            var lookbackTotal = MacdExtLookback(optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod,
                optInSignalMAType);

            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Allocate intermediate buffer for fast/slow MA.
            var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
            Span<T> fastMABuffer = new T[tempInteger];
            Span<T> slowMABuffer = new T[tempInteger];

            /* Calculate the slow MA.
             *
             * Move back the startIdx to get enough data for the signal period.
             * That way, once the signal calculation is done, all the output will start at the requested 'startIdx'.
             */
            tempInteger = startIdx - lookbackSignal;
            var retCode = MaImpl(inReal, new Range(tempInteger, endIdx), slowMABuffer, out var outRange1, optInSlowPeriod, optInSlowMAType);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            // Calculate the fast MA.
            retCode = MaImpl(inReal, new Range(tempInteger, endIdx), fastMABuffer, out _, optInFastPeriod, optInFastMAType);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            var nbElement1 = outRange1.End.Value - outRange1.Start.Value;
            // Calculate (fast MA) - (slow MA).
            for (var i = 0; i < nbElement1; i++)
            {
                fastMABuffer[i] -= slowMABuffer[i];
            }

            // Copy the result into the output for the caller.
            fastMABuffer.Slice(lookbackSignal, endIdx - startIdx + 1).CopyTo(outMACD);

            // Calculate the signal/trigger line.
            retCode = MaImpl(fastMABuffer, Range.EndAt(nbElement1 - 1), outMACDSignal, out var outRange2, optInSignalPeriod, optInSignalMAType);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            var nbElement2 = outRange2.End.Value - outRange2.Start.Value;
            // Calculate the histogram.
            for (var i = 0; i < nbElement2; i++)
            {
                outMACDHist[i] = outMACD[i] - outMACDSignal[i];
            }

            outRange = new Range(startIdx, startIdx + nbElement2);

            return Core.RetCode.Success;
        }
    }

}
