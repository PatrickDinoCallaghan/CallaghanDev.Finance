using CallaghanDev.Finance.TechnicalAnalysis.Helpers;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        /// <summary>
        /// Vector Trigonometric ATan (Math Transform)
        /// </summary>
        /// <param name="inReal">A span of input values.</param>
        /// <param name="inRange">The range of indices that determines the portion of data to be calculated within the input spans.</param>
        /// <param name="outReal">A span to store the calculated values.</param>
        /// <param name="outRange">The range of indices representing the valid data within the output spans.</param>
        /// <typeparam name="T">
        /// The numeric data type, typically <see langword="float"/> or <see langword="double"/>,
        /// implementing the <see cref="IFloatingPointIeee754{T}"/> interface.
        /// </typeparam>
        /// <returns>
        /// A <see cref="Core.RetCode"/> value indicating the success or failure of the calculation.
        /// Returns <see cref="Core.RetCode.Success"/> on successful calculation, or an appropriate error code otherwise.
        /// </returns>
        /// <remarks>
        /// ATan applies the arctangent (inverse tangent) function to each data point in a series, primarily for advanced mathematical modeling,
        /// rather than standard technical analysis.
        /// <para>
        /// The function is rarely used alone for generating signals. It may be integrated into specialized or proprietary models,
        /// in combination with other mathematical transformations.
        /// </para>
        /// </remarks>
        public static Core.RetCode Atan<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AtanImpl(inReal, inRange, outReal, out outRange);

        /// <summary>
        /// Returns the lookback period for <see cref="Atan{T}">Atan</see>.
        /// </summary>
        /// <returns>Always 0 since no historical data is required for this calculation.</returns>
        public static int AtanLookback() => 0;

        /// <remarks>
        /// For compatibility with abstract API
        /// </remarks>
        private static Core.RetCode Atan<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange) where T : IFloatingPointIeee754<T> =>
            AtanImpl<T>(inReal, inRange, outReal, out outRange);

        private static Core.RetCode AtanImpl<T>(
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
                outReal[outIdx++] = T.Atan(inReal[i]);
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }
    }
}