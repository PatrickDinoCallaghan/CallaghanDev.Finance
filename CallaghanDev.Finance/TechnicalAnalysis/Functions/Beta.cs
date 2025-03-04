using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode Beta<T>(
            ReadOnlySpan<T> inReal0,
            ReadOnlySpan<T> inReal1,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
            BetaImpl(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);


        public static int BetaLookback(int optInTimePeriod = 5) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

        private static Core.RetCode Beta<T>(
            T[] inReal0,
            T[] inReal1,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
            BetaImpl<T>(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode BetaImpl<T>(
            ReadOnlySpan<T> inReal0,
            ReadOnlySpan<T> inReal1,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            /* The Beta 'algorithm' is a measure of a stocks volatility vs from index. The stock prices are given in inReal0 and
             * the index prices are give in inReal1. The size of these vectors should be equal.
             * The algorithm is to calculate the change between prices in both vectors and then 'plot' these changes
             * are points in the Euclidean plane. The x value of the point is market return and the y value is the security return.
             * The beta value is the slope of a linear regression through these points. A beta of 1 is simple the line y=x,
             * so the stock varies precisely with the market. A beta of less than one means the stock varies less than
             * the market and a beta of more than one means the stock varies more than market.
             */

            var lookbackTotal = BetaLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            T sxy, sx, sy;
            var sxx = sxy = sx = sy = T.Zero;
            var trailingIdx = startIdx - lookbackTotal;
            var trailingLastPriceX = inReal0[trailingIdx]; // same as lastPriceX except used to remove elements from the trailing summation
            var lastPriceX = trailingLastPriceX; // the last price read from inReal0
            var trailingLastPriceY = inReal1[trailingIdx]; // same as lastPriceY except used to remove elements from the trailing summation
            var lastPriceY = trailingLastPriceY; /* the last price read from inReal1 */

            var i = ++trailingIdx;
            while (i < startIdx)
            {
                UpdateSummation(inReal0, inReal1, ref lastPriceX, ref lastPriceY, ref i, ref sxx, ref sxy, ref sx, ref sy);
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);

            int outIdx = default;
            do
            {
                UpdateSummation(inReal0, inReal1, ref lastPriceX, ref lastPriceY, ref i, ref sxx, ref sxy, ref sx, ref sy);

                UpdateTrailingSummation(inReal0, inReal1, ref trailingLastPriceX, ref trailingLastPriceY, ref trailingIdx, ref sxx, ref sxy,
                    ref sx, ref sy, timePeriod, outReal, ref outIdx);
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static void UpdateSummation<T>(
            ReadOnlySpan<T> real0,
            ReadOnlySpan<T> real1,
            ref T lastPriceX,
            ref T lastPriceY,
            ref int idx,
            ref T sxx,
            ref T sxy,
            ref T sx,
            ref T sy) where T : IFloatingPointIeee754<T>
        {
            var tmpReal = real0[idx];
            var x = !T.IsZero(lastPriceX) ? (tmpReal - lastPriceX) / lastPriceX : T.Zero;
            lastPriceX = tmpReal;

            tmpReal = real1[idx++];
            var y = !T.IsZero(lastPriceY) ? (tmpReal - lastPriceY) / lastPriceY : T.Zero;
            lastPriceY = tmpReal;

            sxx += x * x;
            sxy += x * y;
            sx += x;
            sy += y;
        }

        private static void UpdateTrailingSummation<T>(
            ReadOnlySpan<T> real0,
            ReadOnlySpan<T> real1,
            ref T trailingLastPriceX,
            ref T trailingLastPriceY,
            ref int trailingIdx,
            ref T sxx,
            ref T sxy,
            ref T sx,
            ref T sy,
            T timePeriod,
            Span<T> outReal,
            ref int outIdx) where T : IFloatingPointIeee754<T>
        {
            // Always read the trailing before writing the output because the input and output buffer can be the same.
            var tmpReal = real0[trailingIdx];
            var x = !T.IsZero(trailingLastPriceX) ? (tmpReal - trailingLastPriceX) / trailingLastPriceX : T.Zero;
            trailingLastPriceX = tmpReal;

            tmpReal = real1[trailingIdx++];
            var y = !T.IsZero(trailingLastPriceY) ? (tmpReal - trailingLastPriceY) / trailingLastPriceY : T.Zero;
            trailingLastPriceY = tmpReal;

            tmpReal = timePeriod * sxx - sx * sx;
            outReal[outIdx++] = !T.IsZero(tmpReal) ? (timePeriod * sxy - sx * sy) / tmpReal : T.Zero;

            // Remove the calculation starting with the trailingIdx.
            sxx -= x * x;
            sxy -= x * y;
            sx -= x;
            sy -= y;
        }
    }
}