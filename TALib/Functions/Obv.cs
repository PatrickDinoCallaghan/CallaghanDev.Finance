using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {

        public static Core.RetCode Obv<T>(
            ReadOnlySpan<T> inReal,
            ReadOnlySpan<T> inVolume,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            ObvImpl(inReal, inVolume, inRange, outReal, out outRange);


        public static int ObvLookback() => 0;





        private static Core.RetCode Obv<T>(
            T[] inReal,
            T[] inVolume,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            ObvImpl<T>(inReal, inVolume, inRange, outReal, out outRange);

        private static Core.RetCode ObvImpl<T>(
            ReadOnlySpan<T> inReal,
            ReadOnlySpan<T> inVolume,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length, inVolume.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            var prevOBV = inVolume[startIdx];
            var prevReal = inReal[startIdx];
            int outIdx = default;

            for (var i = startIdx; i <= endIdx; i++)
            {
                var tempReal = inReal[i];
                if (tempReal > prevReal)
                {
                    prevOBV += inVolume[i];
                }
                else if (tempReal < prevReal)
                {
                    prevOBV -= inVolume[i];
                }

                outReal[outIdx++] = prevOBV;
                prevReal = tempReal;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
