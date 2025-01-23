using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Cci<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            CciImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);


        public static int CciLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        
        
        

        private static Core.RetCode Cci<T>(
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            CciImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode CciImpl<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            var lookbackTotal = CciLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Allocate a circular buffer equal to the requested period.
            Span<T> circBuffer = new T[optInTimePeriod];
            int circBufferIdx = default;
            var maxIdxCircBuffer = optInTimePeriod - 1;

            // Do the MA calculation using tight loops.

            // Add-up the initial period, except for the last value. Fill up the circular buffer at the same time.
            var i = startIdx - lookbackTotal;
            while (i < startIdx)
            {
                circBuffer[circBufferIdx++] = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
                i++;
                if (circBufferIdx > maxIdxCircBuffer)
                {
                    circBufferIdx = 0;
                }
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);
            var tPointZeroOneFive = T.CreateChecked(0.015);

            // Proceed with the calculation for the requested range.
            // The algorithm allows the input and output to be the same buffer.
            int outIdx = default;
            do
            {
                var lastValue = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
                circBuffer[circBufferIdx++] = lastValue;

                // Calculate the average for the whole period.
                var theAverage = CalcAverage(circBuffer, timePeriod);

                // Do the summation of the Abs(TypePrice - average) for the whole period.
                var tempReal2 = CalcSummation(circBuffer, theAverage);

                var tempReal = lastValue - theAverage;
                outReal[outIdx++] = !T.IsZero(tempReal) && !T.IsZero(tempReal2)
                    ? tempReal / (tPointZeroOneFive * (tempReal2 / timePeriod))
                    : T.Zero;

                // Move forward the circular buffer indexes.
                if (circBufferIdx > maxIdxCircBuffer)
                {
                    circBufferIdx = 0;
                }

                i++;
            } while (i <= endIdx);

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static T CalcAverage<T>(Span<T> circBuffer, T timePeriod) where T : IFloatingPointIeee754<T>
        {
            var theAverage = T.Zero;
            foreach (var t in circBuffer)
            {
                theAverage += t;
            }

            theAverage /= timePeriod;
            return theAverage;
        }

        private static T CalcSummation<T>(Span<T> circBuffer, T theAverage) where T : IFloatingPointIeee754<T>
        {
            var tempReal2 = T.Zero;
            foreach (var t in circBuffer)
            {
                tempReal2 += T.Abs(t - theAverage);
            }

            return tempReal2;
        }
    }

}