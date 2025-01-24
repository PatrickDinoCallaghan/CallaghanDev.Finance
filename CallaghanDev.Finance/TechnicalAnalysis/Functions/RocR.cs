using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode RocR<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
            RocRImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int RocRLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;





        private static Core.RetCode RocR<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
            RocRImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode RocRImpl<T>(
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

            var lookbackTotal = RocRLookback(optInTimePeriod);
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
                outReal[outIdx++] = !T.IsZero(tempReal) ? inReal[inIdx] / tempReal : T.Zero;
                inIdx++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
