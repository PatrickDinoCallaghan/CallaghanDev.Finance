using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode Div<T>(
            ReadOnlySpan<T> inReal0,
            ReadOnlySpan<T> inReal1,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            DivImpl(inReal0, inReal1, inRange, outReal, out outRange);


        public static int DivLookback() => 0;

        
        
        

        private static Core.RetCode Div<T>(
            T[] inReal0,
            T[] inReal1,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            DivImpl<T>(inReal0, inReal1, inRange, outReal, out outRange);

        private static Core.RetCode DivImpl<T>(
            ReadOnlySpan<T> inReal0,
            ReadOnlySpan<T> inReal1,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (startIdx, endIdx) = rangeIndices;

            int outIdx = default;
            for (var i = startIdx; i <= endIdx; i++)
            {
                outReal[outIdx++] = inReal0[i] / inReal1[i];
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}

