using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode MacdFix<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMACD,
            Span<T> outMACDSignal,
            Span<T> outMACDHist,
            out Range outRange,
            int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
            MacdFixImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInSignalPeriod);


        public static int MacdFixLookback(int optInSignalPeriod = 9) => EmaLookback(26) + EmaLookback(optInSignalPeriod);

        private static Core.RetCode MacdFix<T>(
            T[] inReal,
            Range inRange,
            T[] outMACD,
            T[] outMACDSignal,
            T[] outMACDHist,
            out Range outRange,
            int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
            MacdFixImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInSignalPeriod);

        private static Core.RetCode MacdFixImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outMACD,
            Span<T> outMACDSignal,
            Span<T> outMACDHist,
            out Range outRange,
            int optInSignalPeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is null)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            if (optInSignalPeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            return FunctionHelpers.CalcMACD(
                inReal,
                inRange,
                outMACD,
                outMACDSignal,
                outMACDHist,
                out outRange,
                0, /* 0 indicate fix 12 == 0.15  for optInFastPeriod */
                0, /* 0 indicate fix 26 == 0.075 for optInSlowPeriod */
                optInSignalPeriod);
        }
    }

}
