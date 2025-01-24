using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode Atr<T>(
            ReadOnlySpan<T> inHigh,
            ReadOnlySpan<T> inLow,
            ReadOnlySpan<T> inClose,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AtrImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);


        public static int AtrLookback(int optInTimePeriod = 14) =>
            optInTimePeriod < 1 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Atr);

        
        
        

        private static Core.RetCode Atr<T>(
            T[] inHigh,
            T[] inLow,
            T[] inClose,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            AtrImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode AtrImpl<T>(
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

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            /* Average True Range is the greatest of the following:
             *
             *   val1 = distance from today's high to today's low.
             *   val2 = distance from yesterday's close to today's high.
             *   val3 = distance from yesterday's close to today's low.
             *
             * These value are averaged for the specified period using Wilder method.
             * The method has an unstable period comparable to and Exponential Moving Average (EMA).
             */

            var lookbackTotal = AtrLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            // Trap the case where no smoothing is needed.
            if (optInTimePeriod == 1)
            {
                return TRange(inHigh, inLow, inClose, inRange, outReal, out outRange);
            }

            Span<T> tempBuffer = new T[lookbackTotal + (endIdx - startIdx) + 1];
            var retCode = TRangeImpl(inHigh, inLow, inClose, new Range(startIdx - lookbackTotal + 1, endIdx), tempBuffer, out _);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            // First value of the ATR is a simple Average of the TRange output for the specified period.
            Span<T> prevATRTemp = new T[1];
            retCode = FunctionHelpers.CalcSimpleMA(tempBuffer, new Range(optInTimePeriod - 1, optInTimePeriod - 1), prevATRTemp, out _,
                optInTimePeriod);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);

            var prevATR = prevATRTemp[0];

            /* Subsequent value are smoothed using the previous ATR value (Wilder's approach).
             *   1) Multiply the previous ATR by 'period - 1'.
             *   2) Add today TR value.
             *   3) Divide by 'period'.
             */
            var today = optInTimePeriod;
            var outIdx = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Atr);
            // Skip the unstable period.
            while (outIdx != 0)
            {
                prevATR *= timePeriod - T.One;
                prevATR += tempBuffer[today++];
                prevATR /= timePeriod;
                outIdx--;
            }

            outIdx = 1;
            outReal[0] = prevATR;

            // Do the number of requested ATR.
            var nbATR = endIdx - startIdx + 1;

            while (--nbATR != 0)
            {
                prevATR *= timePeriod - T.One;
                prevATR += tempBuffer[today++];
                prevATR /= timePeriod;
                outReal[outIdx++] = prevATR;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return retCode;
        }
    }

}
