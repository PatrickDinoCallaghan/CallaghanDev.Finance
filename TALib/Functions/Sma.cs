using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Sma<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            SmaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int SmaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode Sma<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            SmaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode SmaImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is null)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            if (optInTimePeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            return FunctionHelpers.CalcSimpleMA(inReal, inRange, outReal, out outRange, optInTimePeriod);
        }
    }

}