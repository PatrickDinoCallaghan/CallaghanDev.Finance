using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {
        public static Core.RetCode Cosh<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            CoshImpl(inReal, inRange, outReal, out outRange);


        public static int CoshLookback() => 0;

        
        
        

        private static Core.RetCode Cosh<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            CoshImpl<T>(inReal, inRange, outReal, out outRange);

        private static Core.RetCode CoshImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            int outIdx = default;
            for (var i = startIdx; i <= endIdx; i++)
            {
                outReal[outIdx++] = T.Cosh(inReal[i]);
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}
