using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        /// <summary>
        /// Vector Trigonometric ACos (Math Transform)
        /// </summary>
        /// <typeparam name="T">
        /// The numeric data type, typically <see cref="float"/> or <see cref="double"/>,
        /// implementing the <see cref="IFloatingPointIeee754{T}"/> interface.
        /// </typeparam>
        /// <param name="inReal">A span of input values.</param>
        /// <param name="inRange">A range of indices that determines the portion of data to be calculated within the input span.</param>
        /// <param name="outReal">The span in which to store the calculated values.</param>
        /// <param name="outRange">The range of indices that represent the valid portion of data within the output span.</param>
        /// <returns>
        /// A <see cref="Core.RetCode"/> value indicating the success or failure of the calculation.
        /// Returns <see cref="Core.RetCode.Success"/> on successful calculation, or an appropriate error code otherwise.
        /// </returns>
        
        /// The arc cosine is the inverse function of the cosine, returning the angle in radians whose cosine is the input value.
        

        public static Core.RetCode Acos<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AcosImpl(inReal, inRange, outReal, out outRange);

        /// <summary>
        /// Returns the lookback period for <see cref="Acos{T}"/>.
        /// </summary>
        /// <returns>Always 0 since no historical data is required for this calculation.</returns>

        public static int AcosLookback() => 0;

        
        
        

        private static Core.RetCode Acos<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AcosImpl<T>(inReal, inRange, outReal, out outRange);

        private static Core.RetCode AcosImpl<T>(
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

            var outIdx = 0;
            for (var i = startIdx; i <= endIdx; i++)
            {
                outReal[outIdx++] = T.Acos(inReal[i]);
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
