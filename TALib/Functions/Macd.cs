using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {

        public static Core.RetCode Macd<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMACD,
            Span<T> outMACDSignal,
            Span<T> outMACDHist,
            out Range outRange,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26,
            int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
            MacdImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod, optInSignalPeriod);


        public static int MacdLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
            {
                return -1;
            }

            if (optInSlowPeriod < optInFastPeriod)
            {
                optInSlowPeriod = optInFastPeriod;
            }

            return EmaLookback(optInSlowPeriod) + EmaLookback(optInSignalPeriod);
        }

        private static Core.RetCode Macd<T>(
            T[] inReal,
            Range inRange,
            T[] outMACD,
            T[] outMACDSignal,
            T[] outMACDHist,
            out Range outRange,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26,
            int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
            MacdImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod,
                optInSignalPeriod);

        private static Core.RetCode MacdImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMACD,
            Span<T> outMACDSignal,
            Span<T> outMACDHist,
            out Range outRange,
            int optInFastPeriod,
            int optInSlowPeriod,
            int optInSignalPeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is null)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            return FunctionHelpers.CalcMACD(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod,
                optInSlowPeriod, optInSignalPeriod);
        }
    }

}