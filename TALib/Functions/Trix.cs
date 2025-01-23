using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {

        public static Core.RetCode Trix<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            TrixImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int TrixLookback(int optInTimePeriod = 30) =>
            optInTimePeriod < 1 ? -1 : EmaLookback(optInTimePeriod) * 3 + RocRLookback(1);

        private static Core.RetCode Trix<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
            TrixImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode TrixImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            if (optInTimePeriod < 1)
            {
                return Core.RetCode.BadParam;
            }

            var emaLookback = EmaLookback(optInTimePeriod);
            var lookbackTotal = TrixLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            var outBegIdx = startIdx;
            var nbElementToOutput = endIdx - startIdx + 1 + lookbackTotal;
            Span<T> tempBuffer = new T[nbElementToOutput];

            var k = FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One);
            var retCode = FunctionHelpers.CalcExponentialMA(inReal, new Range(startIdx - lookbackTotal, endIdx), tempBuffer, out var range,
                optInTimePeriod, k);
            if (retCode != Core.RetCode.Success || range.End.Value == 0)
            {
                return retCode;
            }

            nbElementToOutput--;

            nbElementToOutput -= emaLookback;
            retCode = FunctionHelpers.CalcExponentialMA(tempBuffer, Range.EndAt(nbElementToOutput), tempBuffer, out range, optInTimePeriod, k);
            if (retCode != Core.RetCode.Success || range.End.Value == 0)
            {
                return retCode;
            }

            nbElementToOutput -= emaLookback;
            retCode = FunctionHelpers.CalcExponentialMA(tempBuffer, Range.EndAt(nbElementToOutput), tempBuffer, out range, optInTimePeriod, k);
            if (retCode != Core.RetCode.Success || range.End.Value == 0)
            {
                return retCode;
            }

            // Calculate the 1-day Rate-Of-Change
            nbElementToOutput -= emaLookback;
            retCode = RocImpl(tempBuffer, Range.EndAt(nbElementToOutput), outReal, out range, 1);
            if (retCode != Core.RetCode.Success || range.End.Value == 0)
            {
                return retCode;
            }

            outRange = new Range(outBegIdx, outBegIdx + range.End.Value - range.Start.Value);

            return Core.RetCode.Success;
        }
    }

}
