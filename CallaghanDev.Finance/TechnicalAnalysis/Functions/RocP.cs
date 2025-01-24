using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{

    public static partial class Functions
    {

        public static Core.RetCode RocP<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
            RocPImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int RocPLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

        private static Core.RetCode RocP<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
            RocPImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode RocPImpl<T>(
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

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            /* Roc and RocP are centered at zero and can have positive and negative value. Here are some equivalence:
             *   ROC = ROCP/100
             *       = ((price - prevPrice) / prevPrice) / 100
             *       = ((price / prevPrice) - 1) * 100
             *
             * RocR and RocR100 are ratio respectively centered at 1 and 100 and are always positive values.
             */

            var lookbackTotal = RocPLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            int outIdx = default;
            var inIdx = startIdx;
            var trailingIdx = startIdx - lookbackTotal;
            while (inIdx <= endIdx)
            {
                var tempReal = inReal[trailingIdx++];
                outReal[outIdx++] = !T.IsZero(tempReal) ? (inReal[inIdx] - tempReal) / tempReal : T.Zero;
                inIdx++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}