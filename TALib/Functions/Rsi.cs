using System.Numerics;
using TALib.Helpers;

namespace TALib
{
    public static partial class Functions
    {
        public static Core.RetCode Rsi<T>(
            ReadOnlySpan<T> inReal,
            Range inRange,
            Span<T> outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            RsiImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);


        public static int RsiLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2)
            {
                return -1;
            }

            var retValue = optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Rsi);
            if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock)
            {
                retValue--;
            }

            return retValue;
        }

        private static Core.RetCode Rsi<T>(
            T[] inReal,
            Range inRange,
            T[] outReal,
            out Range outRange,
            int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
            RsiImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

        private static Core.RetCode RsiImpl<T>(
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

            /* The following algorithm is base on the original work from Wilder's and shall represent
             * the original idea behind the classic RSI.
             *
             * Metastock is starting the calculation one price bar earlier.
             * To make this possible, they assume that the very first bar will be identical to the previous one (no gain or loss).
             */

            var lookbackTotal = RsiLookback(optInTimePeriod);
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
            if (Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Rsi) == 0 &&
                Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock &&
                ProcessRsiMetastockCompatibility(inReal, outReal, ref outRange, optInTimePeriod, endIdx, startIdx, ref prevValue, ref today,
                    ref outIdx, out var retCode))
            {
                return retCode;
            }

            // Remaining of the processing is identical.
            InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);

            /* Subsequent prevLoss and prevGain are smoothed using the previous values (Wilder's approach).
             *   1) Multiply the previous by 'period - 1'.
             *   2) Add today value.
             *   3) Divide by 'period'.
             */
            prevLoss /= timePeriod;
            prevGain /= timePeriod;

            /* Often documentation present the RSI calculation as follows:
             *   RSI = 100 - (100 / 1 + (prevGain / prevLoss))
             *
             * The following is equivalent:
             *   RSI = 100 * (prevGain / (prevGain + prevLoss))
             *
             * The second equation is used here for speed optimization.
             */
            if (today > startIdx)
            {
                var tempValue1 = prevGain + prevLoss;
                outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * (prevGain / tempValue1) : T.Zero;
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
                outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * (prevGain / tempValue1) : T.Zero;
            }

            outRange = new Range(startIdx, startIdx + outIdx);

            return Core.RetCode.Success;
        }

        private static bool ProcessRsiMetastockCompatibility<T>(
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
            WriteInitialRsiValue(prevGain, prevLoss, optInTimePeriod, outReal, ref outIdx);

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

        private static void InitGainsAndLosses<T>(
            ReadOnlySpan<T> real,
            ref int today,
            ref T prevValue,
            int optInTimePeriod,
            out T prevGain,
            out T prevLoss) where T : IFloatingPointIeee754<T>
        {
            prevGain = T.Zero;
            prevLoss = T.Zero;
            today++;
            for (var i = optInTimePeriod; i > 0; i--)
            {
                var tempValue1 = real[today++];
                var tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;

                if (tempValue2 < T.Zero)
                {
                    prevLoss -= tempValue2;
                }
                else
                {
                    prevGain += tempValue2;
                }
            }
        }

        private static void WriteInitialRsiValue<T>(
            T prevGain,
            T prevLoss,
            int optInTimePeriod,
            Span<T> outReal,
            ref int outIdx) where T : IFloatingPointIeee754<T>
        {
            var timePeriod = T.CreateChecked(optInTimePeriod);

            var tempValue1 = prevLoss / timePeriod;
            var tempValue2 = prevGain / timePeriod;

            tempValue1 = tempValue2 + tempValue1;
            outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * (tempValue2 / tempValue1) : T.Zero;
        }

        private static void ProcessToday<T>(
            ReadOnlySpan<T> real,
            ref int today,
            ref T prevValue,
            ref T prevGain,
            ref T prevLoss,
            T timePeriod) where T : IFloatingPointIeee754<T>
        {
            var tempValue1 = real[today++];
            var tempValue2 = tempValue1 - prevValue;
            prevValue = tempValue1;

            prevLoss *= timePeriod - T.One;
            prevGain *= timePeriod - T.One;

            if (tempValue2 < T.Zero)
            {
                prevLoss -= tempValue2;
            }
            else
            {
                prevGain += tempValue2;
            }

            prevLoss /= timePeriod;
            prevGain /= timePeriod;
        }
    }

}
