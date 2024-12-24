using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode LinearRegSlope<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            LinearRegSlopeImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int LinearRegSlopeLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode LinearRegSlope<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            LinearRegSlopeImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode LinearRegSlopeImpl<T>(
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

            /* Linear Regression is a concept also known as the "least squares method" or "best fit."
             * Linear Regression attempts to fit a straight line between several data points in such a way that
             * distance between each data point and the line is minimized.
             *
             * For each point, a straight line over the specified previous bar period is determined in terms of y = b + m * x:
             *
             * Returns 'm'
             */

            var lookbackTotal = LinearRegSlopeLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            int outIdx = default;
            var today = startIdx;

            var timePeriod = T.CreateChecked(optInTimePeriod);
            var sumX = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * 0.5);
            var sumXSqr = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * (optInTimePeriod * 2 - 1) / 6.0);
            var divisor = sumX * sumX - timePeriod * sumXSqr;
            while (today <= endIdx)
            {
                T sumXY = T.Zero, sumY = T.Zero;
                for (var i = optInTimePeriod; i-- != 0;)
                {
                    var tempValue1 = inReal[today - i];
                    sumY += tempValue1;
                    sumXY += T.CreateChecked(i) * tempValue1;
                }

                outReal[outIdx++] = (timePeriod * sumXY - sumX * sumY) / divisor;
                today++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
