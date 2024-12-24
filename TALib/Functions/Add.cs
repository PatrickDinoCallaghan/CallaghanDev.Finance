using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        /// <summary>
        /// Vector Arithmetic Add (Math Operators)
        /// </summary>
        /// <typeparam name="T">
        /// The numeric data type, typically <see cref="float"/> or <see cref="double"/>,
        /// implementing the <see cref="IFloatingPointIeee754{T}"/> interface.
        /// </typeparam>
        /// <param name="inReal0">The first span of input values.</param>
        /// <param name="inReal1">The second span of input values.</param>
        /// <param name="inRange">A range of indices that determines the portion of data to be calculated within the input spans.</param>
        /// <param name="outReal">The span in which to store the calculated values.</param>
        /// <param name="outRange">The range of indices that represent the valid portion of data within the output span.</param>
        /// <returns>
        /// A <see cref="Core.RetCode"/> value indicating the success or failure of the calculation.
        /// Returns <see cref="Core.RetCode.Success"/> on successful calculation, or an appropriate error code otherwise.
        /// </returns>
        
        /// The function computes the element-wise addition of two input spans, storing the results in the output span.
        

        public static Core.RetCode Add<T>(
            ReadOnlySpan<T> inReal0,
            ReadOnlySpan<T> inReal1,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AddImpl(inReal0, inReal1, inRange, outReal, out outRange);

        /// <summary>
        /// Returns the lookback period for <see cref="Add{T}"/>.
        /// </summary>
        /// <returns>Always 0 since no historical data is required for this calculation.</returns>

        public static int AddLookback() => 0;

        
        
        

        private static Core.RetCode Add<T>(
            T[] inReal0,
            T[] inReal1,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AddImpl<T>(inReal0, inReal1, inRange, outReal, out outRange);

        private static Core.RetCode AddImpl<T>(
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
                outReal[outIdx++] = inReal0[i] + inReal1[i];
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }

}
