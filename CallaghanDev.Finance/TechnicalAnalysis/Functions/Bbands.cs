using System.Numerics;
using CallaghanDev.Finance.TechnicalAnalysis.Helpers;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Functions
    {

        public static Core.RetCode Bbands<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outRealUpperBand,
            Span<T> outRealMiddleBand,
            Span<T> outRealLowerBand,
            out Range outRange,
            int optInTimePeriod = 5,
            double optInNbDevUp = 2.0,
            double optInNbDevDn = 2.0,
            Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            BbandsImpl(inReal, inRange, outRealUpperBand, outRealMiddleBand, outRealLowerBand, out outRange, optInTimePeriod, optInNbDevUp,
                optInNbDevDn, optInMAType);


        public static int BbandsLookback(int optInTimePeriod = 5, Core.MAType optInMAType = Core.MAType.Sma) =>
            optInTimePeriod < 2 ? -1 : MaLookback(optInTimePeriod, optInMAType);

        
        
        

        private static Core.RetCode Bbands<T>(
            T[] inReal,
            Range inRange,
            T[] outRealUpperBand,
            T[] outRealMiddleBand,
            T[] outRealLowerBand,
            out Range outRange,
            int optInTimePeriod = 5,
            double optInNbDevUp = 2.0,
            double optInNbDevDn = 2.0,
            Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
            BbandsImpl<T>(inReal, inRange, outRealUpperBand, outRealMiddleBand, outRealLowerBand, out outRange, optInTimePeriod, optInNbDevUp,
                optInNbDevDn, optInMAType);

        private static Core.RetCode BbandsImpl<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outRealUpperBand,
            Span<T> outRealMiddleBand,
            Span<T> outRealLowerBand,
            out Range outRange,
            int optInTimePeriod,
            double optInNbDevUp,
            double optInNbDevDn,
            Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
        {
            outRange = Range.EndAt(0);

            if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
            {
                return Core.RetCode.OutOfRangeParam;
            }

            var (_, endIdx) = rangeIndices;

            if (optInTimePeriod < 2 || optInNbDevUp < 0 || optInNbDevDn < 0)
            {
                return Core.RetCode.BadParam;
            }

            /* Identify two temporary buffers among the outputs.
             * These temporary buffers allows to perform the calculation without any memory allocation.
             * Whenever possible, make the tempBuffer1 be the middle band output. This will save one copy operation.
             */

            Span<T> tempBuffer1 = outRealMiddleBand;
            Span<T> tempBuffer2;

            if (inReal == outRealUpperBand)
            {
                tempBuffer2 = outRealLowerBand;
            }
            else
            {
                tempBuffer2 = outRealUpperBand;

                if (inReal == outRealMiddleBand)
                {
                    tempBuffer1 = outRealLowerBand;
                }
            }

            // Check that the caller is not doing tricky things (like using the input buffer in two output)
            if (tempBuffer1 == inReal || tempBuffer2 == inReal)
            {
                return Core.RetCode.BadParam;
            }

            // Calculate the middle band, which is a moving average.
            // The other two bands will simply add/subtract the standard deviation from this middle band.
            var retCode = MaImpl(inReal, inRange, tempBuffer1, out outRange, optInTimePeriod, optInMAType);
            if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
            {
                return retCode;
            }

            var nbElement = outRange.End.Value - outRange.Start.Value;
            if (optInMAType == Core.MAType.Sma)
            {
                // A small speed optimization by re-using the already calculated SMA.
                CalcStandardDeviation(inReal, tempBuffer1, outRange, tempBuffer2, optInTimePeriod);
            }
            else
            {
                // Calculate the Standard Deviation
                retCode = StdDevImpl(inReal, new Range(outRange.Start.Value, endIdx), tempBuffer2, out outRange, optInTimePeriod, 1.0);
                if (retCode != Core.RetCode.Success)
                {
                    outRange = Range.EndAt(0);

                    return retCode;
                }
            }

            // Copy the MA calculation into the middle band output, unless the calculation was done into it already
            if (tempBuffer1 != outRealMiddleBand)
            {
                tempBuffer1[..nbElement].CopyTo(outRealMiddleBand);
            }

            var nbDevUp = T.CreateChecked(optInNbDevUp);
            var nbDevDn = T.CreateChecked(optInNbDevDn);

            /* Do a tight loop to calculate the upper/lower band at the same time.
             *
             * All the following 5 loops are doing the same,
             * except there is an attempt to speed optimize by eliminating unneeded multiplication.
             */
            if (optInNbDevUp.Equals(optInNbDevDn))
            {
                CalcEqualBands(tempBuffer2, outRealMiddleBand, outRealUpperBand, outRealLowerBand, nbElement, nbDevUp);
            }
            else
            {
                CalcDistinctBands(tempBuffer2, outRealMiddleBand, outRealUpperBand, outRealLowerBand, nbElement, nbDevUp, nbDevDn);
            }

            return Core.RetCode.Success;
        }

        private static void CalcStandardDeviation<T>(
            ReadOnlySpan<T> real,
            ReadOnlySpan<T> movAvg,
            Range movAvgRange,
            Span<T> outReal,
            int optInTimePeriod) where T : IFloatingPointIeee754<T>
        {
            var startSum = movAvgRange.Start.Value + 1 - optInTimePeriod;
            var endSum = movAvgRange.Start.Value;
            var periodTotal2 = T.Zero;
            for (var outIdx = startSum; outIdx < endSum; outIdx++)
            {
                var tempReal = real[outIdx];
                tempReal *= tempReal;
                periodTotal2 += tempReal;
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);
            for (var outIdx = 0; outIdx < movAvgRange.End.Value - movAvgRange.Start.Value; outIdx++, startSum++, endSum++)
            {
                var tempReal = real[endSum];
                tempReal *= tempReal;
                periodTotal2 += tempReal;
                var meanValue2 = periodTotal2 / timePeriod;

                tempReal = real[startSum];
                tempReal *= tempReal;
                periodTotal2 -= tempReal;

                tempReal = movAvg[outIdx];
                tempReal *= tempReal;
                meanValue2 -= tempReal;

                outReal[outIdx] = meanValue2 > T.Zero ? T.Sqrt(meanValue2) : T.Zero;
            }
        }

        private static void CalcEqualBands<T>(
            ReadOnlySpan<T> tempBuffer,
            ReadOnlySpan<T> realMiddleBand,
            Span<T> realUpperBand,
            Span<T> realLowerBand,
            int nbElement,
            T nbDevUp) where T : IFloatingPointIeee754<T>
        {
            if (nbDevUp.Equals(T.One))
            {
                // No standard deviation multiplier needed.
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = tempBuffer[i];
                    var tempReal2 = realMiddleBand[i];
                    realUpperBand[i] = tempReal2 + tempReal;
                    realLowerBand[i] = tempReal2 - tempReal;
                }
            }
            else
            {
                // Upper/lower band use the same standard deviation multiplier.
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = tempBuffer[i] * nbDevUp;
                    var tempReal2 = realMiddleBand[i];
                    realUpperBand[i] = tempReal2 + tempReal;
                    realLowerBand[i] = tempReal2 - tempReal;
                }
            }
        }

        private static void CalcDistinctBands<T>(
            ReadOnlySpan<T> tempBuffer,
            ReadOnlySpan<T> realMiddleBand,
            Span<T> realUpperBand,
            Span<T> realLowerBand,
            int nbElement,
            T nbDevUp,
            T nbDevDn) where T : IFloatingPointIeee754<T>
        {
            if (nbDevUp.Equals(T.One))
            {
                // Only lower band has a standard deviation multiplier.
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = tempBuffer[i];
                    var tempReal2 = realMiddleBand[i];
                    realUpperBand[i] = tempReal2 + tempReal;
                    realLowerBand[i] = tempReal2 - tempReal * nbDevDn;
                }
            }
            else if (nbDevDn.Equals(T.One))
            {
                // Only upper band has a standard deviation multiplier.
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = tempBuffer[i];
                    var tempReal2 = realMiddleBand[i];
                    realLowerBand[i] = tempReal2 - tempReal;
                    realUpperBand[i] = tempReal2 + tempReal * nbDevUp;
                }
            }
            else
            {
                // Upper/lower band have distinctive standard deviation multiplier.
                for (var i = 0; i < nbElement; i++)
                {
                    var tempReal = tempBuffer[i];
                    var tempReal2 = realMiddleBand[i];
                    realUpperBand[i] = tempReal2 + tempReal * nbDevUp;
                    realLowerBand[i] = tempReal2 - tempReal * nbDevDn;
                }
            }
        }
    }
}