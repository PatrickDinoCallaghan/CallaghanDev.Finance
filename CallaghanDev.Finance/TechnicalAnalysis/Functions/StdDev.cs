using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode StdDev<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 5,
            double optInNbDev = 1.0) where T : IFloatingPointIeee754<T> =>
            StdDevImpl(inReal, inRange, outReal, out outRange, optInTimePeriod, optInNbDev);


        public static int StdDevLookback(int optInTimePeriod = 5) => optInTimePeriod < 2 ? -1 : VarLookback(optInTimePeriod);

        private static Core.RetCode StdDev<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 5,
            double optInNbDev = 1.0) where T : IFloatingPointIeee754<T> =>
            StdDevImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod, optInNbDev);

        private static Core.RetCode StdDevImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod,
            double optInNbDev) where T : IFloatingPointIeee754<T>
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

            var retCode = FunctionHelpers.CalcVariance(inReal, inRange, outReal, out outRange, optInTimePeriod);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            var nbElement = outRange.End.Value - outRange.Start.Value;
            // Calculate the square root of each variance, this is the standard deviation.
            // Multiply also by the ratio specified.
            if (!optInNbDev.Equals(1.0))
            {
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = outReal[i];
                    outReal[i] = tempReal > T.Zero ? T.Sqrt(tempReal) * T.CreateChecked(optInNbDev) : T.Zero;
                }
            }
            else
            {
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = outReal[i];
                    outReal[i] = tempReal > T.Zero ? T.Sqrt(tempReal) : T.Zero;
                }
            }

            return Core.RetCode.Success;
        }
    }
}
