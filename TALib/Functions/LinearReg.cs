using System.Numerics;
using TALib.Helpers;

namespace TALib
{

    public static partial class Functions
    {

        public static Core.RetCode LinearReg<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            LinearRegImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int LinearRegLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode LinearReg<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            LinearRegImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode LinearRegImpl<T>(
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
             * Returns b + m * (period - 1)
             */

            var lookbackTotal = LinearRegLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            int outIdx = default;
            var today = startIdx;

            var timePeriod = T.CreateChecked(optInTimePeriod);
            var sumX = timePeriod * (timePeriod - T.One) * T.CreateChecked(0.5);
            var sumXSqr = timePeriod * (timePeriod - T.One) * (timePeriod * FunctionHelpers.Two<T>() - T.One) / T.CreateChecked(6);
            var divisor = sumX * sumX - timePeriod * sumXSqr;
            while (today <= endIdx)
            {
                var sumXY = T.Zero;
                var sumY = T.Zero;
                for (var i = optInTimePeriod; i-- != 0;)
                {
                    var tempValue1 = inReal[today - i];
                    sumY += tempValue1;
                    sumXY += T.CreateChecked(i) * tempValue1;
                }

                var m = (timePeriod * sumXY - sumX * sumY) / divisor;
                var b = (sumY - m * sumX) / timePeriod;
                outReal[outIdx++] = b + m * (timePeriod - T.One);
                today++;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
