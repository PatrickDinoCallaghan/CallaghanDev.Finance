using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Apo<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26,
            Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            ApoImpl(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);


        public static int ApoLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma) =>
            optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : MaLookback(Math.Max(optInSlowPeriod, optInFastPeriod), optInMAType);

        
        
        

        private static Core.RetCode Apo<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26,
            Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            ApoImpl<T>(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

        private static Core.RetCode ApoImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInFastPeriod,
            int optInSlowPeriod,
            Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInFastPeriod < 2 || optInSlowPeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            Span<T> tempBuffer = new T[endIdx - startIdx + 1];

            return FunctionHelpers.CalcPriceOscillator(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType,
                tempBuffer, false);
        }
    }
}
