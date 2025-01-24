using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode AvgDev<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AvgDevImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int AvgDevLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        
        
        

        private static Core.RetCode AvgDev<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AvgDevImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode AvgDevImpl<T>(
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

            var lookbackTotal = AvgDevLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            var today = startIdx;
            if (today > endIdx)
            {
                return Core.RetCode.Success;
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);

            var outBegIdx = today;

            int outIdx = default;
            while (today <= endIdx)
            {
                var todaySum = T.Zero;
                for (var i = 0; i < optInTimePeriod; i++)
                {
                    todaySum += inReal[today - i];
                }

                var todayDev = T.Zero;
                for (var i = 0; i < optInTimePeriod; i++)
                {
                    todayDev += T.Abs(inReal[today - i] - todaySum / timePeriod);
                }

                outReal[outIdx++] = todayDev / timePeriod;
                today++;
            }

            outRange = new Range(outBegIdx, outBegIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}
