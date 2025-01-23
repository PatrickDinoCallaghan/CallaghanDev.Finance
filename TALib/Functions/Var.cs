using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Var<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
            VarImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int VarLookback(int optInTimePeriod = 5) => optInTimePeriod < 1 ? -1 : optInTimePeriod - 1;

        private static Core.RetCode Var<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
            VarImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode VarImpl<T>(
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

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            return FunctionHelpers.CalcVariance(inReal, inRange, outReal, out outRange, optInTimePeriod);
        }
    }

}
