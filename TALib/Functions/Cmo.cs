using System.Numerics;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Cmo<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            CmoImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int CmoLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2)
            {
                return -1;
            }

            var retValue = optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Cmo);
            if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock)
            {
                retValue--;
            }

            return retValue;
        }

        
        
        

        private static Core.RetCode Cmo<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            CmoImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode CmoImpl<T>(
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

            if (optInTimePeriod < 2)
            {
                return Core.RetCode.BadParam;
            }

            /* CMO calculation is mostly identical to RSI.
             *
             * The only difference is in the last step of calculation:
             *
             *   RSI = gain / (gain+loss)
             *   CMO = (gain-loss) / (gain+loss)
             *
             * See the Rsi function for potentially some more info on this algo.
             */

            var lookbackTotal = CmoLookback(optInTimePeriod);
            startIdx = Math.Max(startIdx, lookbackTotal);

            if (startIdx > endIdx)
            {
                return Core.RetCode.Success;
            }

            var timePeriod = T.CreateChecked(optInTimePeriod);

            int outIdx = default;

            // Accumulate Wilder's "Average Gain" and "Average Loss" among the initial period.
            var today = startIdx - lookbackTotal;
            var prevValue = inReal[today];

            // If there is an unstable period, no need to calculate since this first value will be surely skip.
            if (Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Cmo) == 0 &&
                Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock &&
                ProcessCmoMetastockCompatibility(inReal, outReal, ref outRange, optInTimePeriod, endIdx, startIdx, ref prevValue, ref today,
                    ref outIdx, out var retCode))
            {
                return retCode;
            }

            InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);

            /* Subsequent prevLoss and prevGain are smoothed using the previous values (Wilder's approach).
             * 1) Multiply the previous by 'period - 1'.
             * 2) Add today value.
             * 3) Divide by 'period'.
             */
            prevLoss /= timePeriod;
            prevGain /= timePeriod;

            /* Often documentation present the RSI calculation as follows:
             *    RSI = 100 - (100 / 1 + (prevGain / prevLoss))
             *
             * The following is equivalent:
             *    RSI = 100 * (prevGain / (prevGain + prevLoss))
             *
             * The second equation is used here for speed optimization.
             */
            if (today > startIdx)
            {
                var tempValue1 = prevGain + prevLoss;
                outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * ((prevGain - prevLoss) / tempValue1) : T.Zero;
            }
            else
            {
                // Skip the unstable period. Do the processing but do not write it in the output.
                while (today < startIdx)
                {
                    ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
                }
            }

            // Unstable period skipped... now continue processing if needed.
            while (today <= endIdx)
            {
                ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
                var tempValue1 = prevGain + prevLoss;
                outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * ((prevGain - prevLoss) / tempValue1) : T.Zero;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool ProcessCmoMetastockCompatibility<T>(
            ReadOnlySpan<T> inReal,
            Span<T> outReal,
            ref Range outRange,
            int optInTimePeriod,
            int endIdx,
            int startIdx,
            ref T prevValue,
            ref int today,
            ref int outIdx,
            out Core.RetCode retCode)
            where T : IFloatingPointIeee754<T>
        {
            // Preserve prevValue because it may get overwritten by the output.
            // (because output ptr could be the same as input ptr).
            var savePrevValue = prevValue;

            InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);
            WriteInitialCmoValue(prevGain, prevLoss, optInTimePeriod, outReal, ref outIdx);

            if (today > endIdx)
            {
                outRange = new Range(startIdx, startIdx + outIdx);
                retCode = Core.RetCode.Success;

                return true;
            }

            // Start over for the next price bar.
            today -= optInTimePeriod;
            prevValue = savePrevValue;
            retCode = Core.RetCode.Success;

            return false;
        }

        private static void WriteInitialCmoValue<T>(
            T prevGain,
            T prevLoss,
            int optInTimePeriod,
            Span<T> outReal,
            ref int outIdx) where T : IFloatingPointIeee754<T>
        {
            var timePeriod = T.CreateChecked(optInTimePeriod);

            var tempValue1 = prevLoss / timePeriod;
            var tempValue2 = prevGain / timePeriod;
            var tempValue3 = tempValue2 - tempValue1;
            var tempValue4 = tempValue1 + tempValue2;

            outReal[outIdx++] = !T.IsZero(tempValue4) ? FunctionHelpers.Hundred<T>() * (tempValue3 / tempValue4) : T.Zero;
        }
    }

}
