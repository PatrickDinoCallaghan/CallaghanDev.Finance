using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode Ema<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            EmaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int EmaLookback(int optInTimePeriod = 30) =>
            optInTimePeriod < 2 ? -1 : optInTimePeriod - 1 + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Ema);

        private static Core.RetCode Ema<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            EmaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode EmaImpl<T>(
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

            return FunctionHelpers.CalcExponentialMA(inReal, inRange, outReal, out outRange, optInTimePeriod,
                FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One));
        }
    }

}
