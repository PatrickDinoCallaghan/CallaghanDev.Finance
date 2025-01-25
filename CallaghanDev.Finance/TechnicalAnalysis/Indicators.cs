using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using CallaghanDev.Finance.TechnicalAnalysis.Exceptions;
using CallaghanDev.Utilities.Extensions;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    //For each indicator the first element of the returned array corresponds to the oldest data point after the lookback period,
    //and the last element corresponds to the most current datapoint.
    public class Indicators<T> where T : struct, IFloatingPoint<T>
    {
        public float[] Accbands(List<ITradingDataPoint<T>> fxData, int optinTimePeriod = 20)
        {
            int lookback = Functions.AccbandsLookback(optinTimePeriod);
            
            if (fxData == null || fxData.Count < Functions.AccbandsLookback(optinTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({optinTimePeriod}).");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;

            
            var upperBand = new float[NoReturned];
            var middleBand = new float[NoReturned];
            var lowerBand = new float[NoReturned];

            var range = new Range(0, highPrices.Length-1);

            // Call Accbands
            var retCode = Functions.Accbands<float>(
                highPrices,
                lowPrices,
                closePrices,
                range,
                upperBand,
                middleBand,
                lowerBand,
                out var outputRange,
                optInTimePeriod: 20);

            // If successful, calculate signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();
                int x = 0;
                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {

                    if (closePrices[x] > upperBand[x])
                    {
                        signals.Add(1);
                    }
                    else if (closePrices[x] < lowerBand[x])
                    {
                        signals.Add(-1);
                    }
                    else
                    {
                        signals.Add(0);
                    }
                    x++;
                }

                return signals.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating acceleration bands. RetCode: {retCode}");
            }

        }
        public float[] Acos(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.AcosLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator.");
            }

            // Extract close prices as floats
            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())- Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();

            // Validate lengths
            if (PriceChanges.Length < 1)
            {
                throw new Exception("Invalid close price data.");
            }


            int NoReturned = fxData.Count() - lookback;
            
            var acosValues = new float[NoReturned];
            var range = new Range(0, PriceChanges.Length-1);

            // Call Acos function
            var retCode = Functions.Acos<float>(
                PriceChanges,
                range,
                acosValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return acosValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ACos indicator. RetCode: {retCode}");
            }
        }
        public float[] Add(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.AcosLookback();

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator.");
            }

            // Extract two sets of values to be added, e.g., Bid and Offer
            var bidPrices = fxData.Select(d => Convert.ToSingle(d.Bid.GetValueOrDefault())).ToArray().AsSpan();
            var offerPrices = fxData.Select(d => Convert.ToSingle(d.Offer.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (bidPrices.Length != offerPrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;
            
            var addedValues = new float[NoReturned];
            var range = new Range(0, bidPrices.Length-1);

            // Call Add function
            var retCode = Functions.Add<float>(
                bidPrices,
                offerPrices,
                range,
                addedValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return addedValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Add indicator. RetCode: {retCode}");
            }
        }
        public float[] Adx(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.AdxLookback(timePeriod);

            
            if (fxData == null || fxData.Count < Functions.AdxLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }
            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }


            int NoReturned = fxData.Count() - lookback;
            
            var adxValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call Adx function
            var retCode = Functions.Adx<float>(
                highPrices,
                lowPrices,
                closePrices,
                range,
                adxValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return adxValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ADX indicator. RetCode: {retCode}");
            }
        }
        public float[] Adxr(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.AdxrLookback(timePeriod);
            
            if (fxData == null || fxData.Count < Functions.AdxrLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;
            
            var adxrValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call Adxr function
            var retCode = Functions.Adxr<float>(
                highPrices,
                lowPrices,
                closePrices,
                range,
                adxrValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return adxrValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ADXR indicator. RetCode: {retCode}");
            }
        }
        public float[] Apo(List<ITradingDataPoint<T>> fxData, int fastPeriod = 12, int slowPeriod = 26, Core.MAType maType = Core.MAType.Sma)
        {
            int lookback = Functions.ApoLookback(fastPeriod, slowPeriod);
            
            if (fxData == null || fxData.Count < Functions.ApoLookback(fastPeriod, slowPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen fast time period ({fastPeriod}) and the chosen slow time period {slowPeriod}.");
            }
            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var apoValues = new float[NoReturned];
            var range = new Range(0, closePrices.Length-1);

            // Call Apo function
            var retCode = Functions.Apo<float>(
                closePrices,
                range,
                apoValues,
                out var outputRange,
                fastPeriod,
                slowPeriod,
                maType);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return apoValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating APO indicator. RetCode: {retCode}");
            }
        }
        public float[] AroonUp(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            return Aroon(fxData, timePeriod).aroonUp;
        }
        public float[] AroonDown(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            return Aroon(fxData, timePeriod).aroonUp;
        }
        private (float[] aroonUp, float[] aroonDown) Aroon(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.AroonLookback(timePeriod);
            
            if (fxData == null || fxData.Count < Functions.AroonLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }
            // Extract high and low prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;

            
            var aroonUpValues = new float[NoReturned];
            var aroonDownValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call Aroon function
            var retCode = Functions.Aroon<float>(
                highPrices,
                lowPrices,
                range,
                aroonDownValues,
                aroonUpValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (aroonUpValues.ToArray(), aroonDownValues.ToArray()); // Return both arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating Aroon indicator. RetCode: {retCode}");
            }
        }
        public float[] AroonOsc(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.AroonOscLookback(timePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }

            // Extract high and low prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;
            
            var aroonOscValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length-1);

            // Call AroonOsc function
            var retCode = Functions.AroonOsc<float>(
                highPrices,
                lowPrices,
                range,
                aroonOscValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return aroonOscValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Aroon Oscillator. RetCode: {retCode}");
            }
        }
        public float[] Asin(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.AsinLookback();

            
            if (fxData == null || fxData.Count < Functions.AsinLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault()) - Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            
            var asinValues = new float[NoReturned];

            var range = new Range(0, PriceChanges.Length - 1);

            // Call Asin function
            var retCode = Functions.Asin<float>(
                PriceChanges,
                range,
                asinValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return asinValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ASIN indicator. RetCode: {retCode}");
            }
        }
        public float[] Atan(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.AtanLookback();

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault()) - Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var atanValues = new float[NoReturned];

            var range = new Range(0, PriceChanges.Length - 1);

            // Call Atan function
            var retCode = Functions.Atan<float>(
                PriceChanges,
                range,
                atanValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return atanValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ATAN indicator. RetCode: {retCode}");
            }
        }
        public float[] Atr(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            

            int lookback = Functions.AtrLookback();

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;
            
            var atrValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length-1);

            // Call Atr function
            var retCode = Functions.Atr<float>(
                highPrices,
                lowPrices,
                closePrices,
                range,
                atrValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return atrValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ATR indicator. RetCode: {retCode}");
            }
        }
        public float[] AvgDev(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.AvgDevLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var avgDevValues = new float[NoReturned];
            var range = new Range(0, closePrices.Length - 1);

            // Call AvgDev function
            var retCode = Functions.AvgDev<float>(
                closePrices,
                range,
                avgDevValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return avgDevValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Average Deviation indicator. RetCode: {retCode}");
            }
        }
        public float[] AvgPrice(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.AvgPriceLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract open, high, low, and close prices as floats
            var openPrices = fxData.Select(d => Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().AsSpan();
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            // Validate lengths
            if (openPrices.Length != highPrices.Length || highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            int NoReturned = fxData.Count() - lookback;
            
            var avgPriceValues = new float[NoReturned];
            var range = new Range(0, openPrices.Length - 1);

            // Call AvgPrice function
            var retCode = Functions.AvgPrice<float>(
                openPrices,
                highPrices,
                lowPrices,
                closePrices,
                range,
                avgPriceValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return avgPriceValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Average Price indicator. RetCode: {retCode}");
            }
        }

        public float[] BbandsUpperBandSignals(List<ITradingDataPoint<T>> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return Bbands(fxData, timePeriod, nbDevUp, nbDevDn, maType).upperBandSignals;
        }
        public float[] BbandsMiddleBandSignals(List<ITradingDataPoint<T>> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return Bbands(fxData, timePeriod, nbDevUp, nbDevDn, maType).middleBandSignals;
        }
        public float[] BbandsLowerBandSignals(List<ITradingDataPoint<T>> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return Bbands(fxData, timePeriod, nbDevUp, nbDevDn, maType).lowerBandSignals;
        }
        private (float[] upperBandSignals, float[] middleBandSignals, float[] lowerBandSignals) Bbands(List<ITradingDataPoint<T>> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            int lookback = Functions.BbandsLookback();

            
            if (fxData == null || fxData.Count < Functions.BbandsLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            int NoReturned = fxData.Count() - lookback;

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            var upperBandValues = new float[NoReturned];
            var middleBandValues = new float[NoReturned];
            var lowerBandValues = new float[NoReturned];
            var range = new Range(0, closePrices.Length - 1);

            // Call Bbands function
            var retCode = Functions.Bbands<float>(
                closePrices,
                range,
                upperBandValues,
                middleBandValues,
                lowerBandValues,
                out var outputRange,
                timePeriod,
                nbDevUp,
                nbDevDn,
                maType);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (upperBandValues.ToArray(), middleBandValues.ToArray(), lowerBandValues.ToArray());
            }
            else
            {
                throw new Exception($"Error calculating Bollinger Bands. RetCode: {retCode}");
            }
        }

        public float[] Beta(List<ITradingDataPoint<T>> fxData1, List<ITradingDataPoint<T>> fxData2, int timePeriod = 5)
        {
            int lookback = Functions.BetaLookback();
            
            if (fxData1 == null || fxData1.Count < lookback || fxData2.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            if (fxData1.Count != fxData2.Count)
            {
                throw new Exception("Input data lengths must match.");
            }

            // Extract close prices from both data sets
            var closePrices1 = fxData1.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices2 = fxData2.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData1.Count() - lookback;

            
            var betaValues = new float[NoReturned];
            var range = new Range(0, closePrices1.Length - 1);

            // Call Beta function
            var retCode = Functions.Beta<float>(
                closePrices1,
                closePrices2,
                range,
                betaValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return betaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Beta indicator. RetCode: {retCode}");
            }
        }
        public float[] Bop(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.BopLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract open, high, low, and close prices as floats
            var openPrices = fxData.Select(d => Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().AsSpan();
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var bopValues = new float[NoReturned];
            var range = new Range(0, openPrices.Length - 1);

            // Call Bop function
            var retCode = Functions.Bop<float>(
                openPrices,
                highPrices,
                lowPrices,
                closePrices,
                range,
                bopValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return bopValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Balance of Power indicator. RetCode: {retCode}");
            }
        }
        public float[] Cci(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.CciLookback(timePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var cciValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call CCI function
            var retCode = Functions.Cci<float>(
                highPrices,
                lowPrices,
                closePrices,
                range,
                cciValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return cciValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating CCI indicator. RetCode: {retCode}");
            }
        }
        public float[] Ceil(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.CeilLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var ceilValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call Ceil function
            var retCode = Functions.Ceil<float>(
                prices,
                range,
                ceilValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return ceilValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Ceiling indicator. RetCode: {retCode}");
            }
        }
        public float[] Cmo(List<ITradingDataPoint<T>> fxData, int timePeriod = 14)
        {
            int lookback = Functions.CmoLookback(timePeriod);

            
            if (fxData == null || fxData.Count < lookback )
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var cmoValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call CMO function
            var retCode = Functions.Cmo<float>(
                prices,
                range,
                cmoValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return cmoValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating CMO indicator. RetCode: {retCode}");
            }
        }
        public float[] Correl(List<ITradingDataPoint<T>> fxData1, List<ITradingDataPoint<T>> fxData2, int timePeriod = 30)
        {
            int lookback = Functions.CorrelLookback(timePeriod); 
            
            if (fxData1 == null || fxData2 == null || fxData1.Count < lookback || fxData2.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices1 = fxData1.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var prices2 = fxData2.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData1.Count() - lookback;
            
            var correlValues = new float[NoReturned];
            var range = new Range(0, prices1.Length - 1);

            // Call Correl function
            var retCode = Functions.Correl<float>(
                prices1,
                prices2,
                range,
                correlValues,
                out var outputRange,
                timePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return correlValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating CORREL indicator. RetCode: {retCode}");
            }
        }
        public float[] Cos(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.CosLookback();

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var cosValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            var retCode = Functions.Cos<float>(
                prices,
                range,
                cosValues,
                out var outputRange);


            if (retCode == Core.RetCode.Success)
            {
                return cosValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating COS indicator. RetCode: {retCode}");
            }
        }
        public float[] Cosh(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.CoshLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)

            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault()) - Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var coshValues = new float[NoReturned];
            var range = new Range(0, PriceChanges.Length - 1);

            // Call Cosh function
            var retCode = Functions.Cosh<float>(
                PriceChanges,
                range,
                coshValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return coshValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating COSH indicator. RetCode: {retCode}");
            }
        }
        public float[] Dema(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.DemaLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }


            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var demaValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call Dema function
            var retCode = Functions.Dema<float>(
                prices,
                range,
                demaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return demaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating DEMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Div(List<ITradingDataPoint<T>> fxData, List<ITradingDataPoint<T>> fxData2)
        {
            int lookback = Functions.DivLookback();
            
            if (fxData == null || fxData2 == null || fxData.Count < lookback || fxData2.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            if (fxData == null || fxData2 == null || fxData.Count != fxData2.Count || fxData.Count == 0)
            {
                throw new Exception("Input data lists must have the same non-zero length.");
            }

            // Extract prices from fxData and fxData2 (using Offer or Bid values)
            var prices1 = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var prices2 = fxData2.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var divValues = new float[NoReturned];
            var range = new Range(0, prices1.Length - 1);

            // Call Div function
            var retCode = Functions.Div<float>(
                prices1,
                prices2,
                range,
                divValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return divValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Div indicator. RetCode: {retCode}");
            }
        }
        public float[] Dx(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.DxLookback(optInTimePeriod);
            
            if (fxData == null ||  fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, and Close prices
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close)).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var dxValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call Dx function
            var retCode = Functions.Dx<float>(
                highPrices,
                lowPrices,
                closePrices,
                range,
                dxValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return dxValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating DX indicator. RetCode: {retCode}");
            }
        }
        public float[] Ema(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.EmaLookback(optInTimePeriod);

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var emaValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call Ema function
            var retCode = Functions.Ema<float>(
                prices,
                range,
                emaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return emaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating EMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Exp(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.ExpLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault()) - Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var expValues = new float[NoReturned];
            var range = new Range(0, PriceChanges.Length - 1);

            // Call Exp function
            var retCode = Functions.Exp<float>(
                PriceChanges,
                range,
                expValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return expValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Exp indicator. RetCode: {retCode}");
            }
        }
        public float[] Floor(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.FloorLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var floorValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call Floor function
            var retCode = Functions.Floor<float>(
                prices,
                range,
                floorValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return floorValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Floor indicator. RetCode: {retCode}");
            }
        }
        public float[] HtDcPeriod(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.HtDcPeriodLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var htDcPeriodValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call HtDcPeriod function
            var retCode = Functions.HtDcPeriod<float>(
                prices,
                range,
                htDcPeriodValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return htDcPeriodValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating HT DC Period indicator. RetCode: {retCode}");
            }
        }
        public float[] HtDcPhase(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.HtDcPhaseLookback();

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var htDcPhaseValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call HtDcPhase function
            var retCode = Functions.HtDcPhase<float>(
                prices,
                range,
                htDcPhaseValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return htDcPhaseValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating HT DC Phase indicator. RetCode: {retCode}");
            }
        }

        public float[] HtPhasorInPhase(List<ITradingDataPoint<T>> fxData)
        {
            return HtPhasor(fxData).InPhase;
        }
        public float[] HtPhasorQuadrature(List<ITradingDataPoint<T>> fxData)
        {
            return HtPhasor(fxData).Quadrature;
        }
        private (float[] InPhase, float[] Quadrature) HtPhasor(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.HtPhasorLookback();

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            int NoReturned = fxData.Count() - lookback;


            
            var inPhaseValues = new float[NoReturned];
            var quadratureValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call HtPhasor function
            var retCode = Functions.HtPhasor<float>(
                prices,
                range,
                inPhaseValues,
                quadratureValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (inPhaseValues.ToArray(), quadratureValues.ToArray()); // Return the arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Phasor indicator. RetCode: {retCode}");
            }
        }

        public float[] HtSineSine(List<ITradingDataPoint<T>> fxData)
        {
            return HtSine(fxData).Sine;
        }
        public float[] HtSineLeadSine(List<ITradingDataPoint<T>> fxData)
        {
            return HtSine(fxData).LeadSine;
        }
        public (float[] Sine, float[] LeadSine) HtSine(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.HtSineLookback();

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var sineValues = new float[NoReturned];
            var leadSineValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call HtSine function
            var retCode = Functions.HtSine<float>(
                prices,
                range,
                sineValues,
                leadSineValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (sineValues.ToArray(), leadSineValues.ToArray()); // Return the arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Sine indicator. RetCode: {retCode}");
            }
        }

        public float[] HtTrendline(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.HtTrendlineLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var trendlineValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call HtTrendline function
            var retCode = Functions.HtTrendline<float>(
                prices,
                range,
                trendlineValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return trendlineValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating HT Trendline indicator. RetCode: {retCode}");
            }
        }
        public float[] HtTrendMode(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.HtTrendModeLookback();

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray();

            int NoReturned = fxData.Count() - lookback;


            
            var trendModeValues = new int[NoReturned];
            var inputRange = new Range(0, prices.Length-1); // Entire range of input prices

            // Call HtTrendMode function
            var retCode = Functions.HtTrendMode<float>(
                prices.AsSpan(),              // Input prices
                inputRange,                   // Input range
                trendModeValues.AsSpan(),     // Output array
                out var outputRange           // Output range
            );

            // If successful, return the computed trend mode values
            if (retCode == Core.RetCode.Success)
            {
                // Only take the valid range of results from the output
                return trendModeValues.Select(r=>(float)r).ToArray();
            }
            else
            {
                throw new Exception($"Error calculating HT Trend Mode indicator. RetCode: {retCode}");
            }
        }

        public float[] Kama(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.KamaLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var kamaValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call Kama function
            var retCode = Functions.Kama<float>(
                prices,
                range,
                kamaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return kamaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating KAMA indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearReg(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {

            int lookback = Functions.LinearRegLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var linearRegValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call LinearReg function
            var retCode = Functions.LinearReg<float>(
                prices,
                range,
                linearRegValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return linearRegValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearRegAngle(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.LinearRegAngleLookback(optInTimePeriod);

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var linearRegAngleValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call LinearRegAngle function
            var retCode = Functions.LinearRegAngle<float>(
                prices,
                range,
                linearRegAngleValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return linearRegAngleValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Angle indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearRegIntercept(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.LinearRegInterceptLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var linearRegInterceptValues = new float[NoReturned];
            var range = new Range(0, prices.Length - 1);

            // Call LinearRegIntercept function
            var retCode = Functions.LinearRegIntercept<float>(
                prices,
                range,
                linearRegInterceptValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return linearRegInterceptValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Intercept indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearRegSlope(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.LinearRegSlopeLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var linearRegSlopeValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call LinearRegSlope function
            var retCode = Functions.LinearRegSlope<float>(
                prices,
                range,
                linearRegSlopeValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return linearRegSlopeValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Slope indicator. RetCode: {retCode}");
            }
        }
        public float[] Ln(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.LnLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var lnValues = new float[NoReturned];
            var range = new Range(0, prices.Length -1);

            // Call Ln function
            var retCode = Functions.Ln<float>(
                prices,
                range,
                lnValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return lnValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Ln indicator. RetCode: {retCode}");
            }
        }
        public float[] Log10(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.Log10Lookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var log10Values = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call Log10 function
            var retCode = Functions.Log10<float>(
                prices,
                range,
                log10Values,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return log10Values.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Log10 indicator. RetCode: {retCode}");
            }
        }
        public float[] Ma(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30, Core.MAType optInMAType = Core.MAType.Sma)
        {
            int lookback = Functions.MaLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var maValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MA function
            var retCode = Functions.Ma<float>(
                prices,
                range,
                maValues,
                out var outputRange,
                optInTimePeriod,
                optInMAType);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return maValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MA indicator. RetCode: {retCode}");
            }
        }

        public float[] MacdMACD(List<ITradingDataPoint<T>> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return Macd(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).MACD;
        }
        public float[] MacdSignal(List<ITradingDataPoint<T>> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return Macd(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).Signal;
        }
        public float[] MacdHistogram(List<ITradingDataPoint<T>> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return Macd(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).Histogram;
        }
        private (float[] MACD, float[] Signal, float[] Histogram) Macd(List<ITradingDataPoint<T>> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            int lookback = Functions.MacdLookback(optInFastPeriod, optInSlowPeriod, optInSignalPeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{optInSignalPeriod}, Slow Period:{optInSlowPeriod} and signal period:{optInSignalPeriod}");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var macdValues = new float[NoReturned];
            var signalValues = new float[NoReturned];
            var histogramValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MACD function
            var retCode = Functions.Macd<float>(
                prices,
                range,
                macdValues,
                signalValues,
                histogramValues,
                out var outputRange,
                optInFastPeriod,
                optInSlowPeriod,
                optInSignalPeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (macdValues.ToArray(), signalValues.ToArray(), histogramValues.ToArray());
            }
            else
            {
                throw new Exception($"Error calculating MACD indicator. RetCode: {retCode}");
            }
        }

        public float[] MacdExtMacdSignalss(List<ITradingDataPoint<T>> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            return MacdExt(fxData, optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod).MacdSignals;
        }
        public float[] MacdExtMacdHistSignals(List<ITradingDataPoint<T>> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            return MacdExt(fxData, optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod).MacdHistSignals;
        }
        private (float[] MacdSignals, float[] MacdHistSignals) MacdExt(List<ITradingDataPoint<T>> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            
            if (fxData == null || fxData.Count < Functions.MacdExtLookback(optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType,  optInSignalPeriod, optInSignalMAType))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{optInSignalPeriod}, Slow Period:{optInSlowPeriod} and signal period:{optInSignalPeriod}");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            
            var macdValues = new float[fxData.Count];
            var macdSignalValues = new float[fxData.Count];
            var macdHistValues = new float[fxData.Count];
            var range = new Range(0, prices.Length-1);

            // Call MACD function
            var retCode = Functions.MacdExt<float>(
                prices,
                range,
                macdValues,
                macdSignalValues,
                macdHistValues,
                out var outputRange,
                optInFastPeriod,
                optInFastMAType,
                optInSlowPeriod,
                optInSlowMAType,
                optInSignalPeriod,
                optInSignalMAType);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var macdSignals = new List<float>();
                var macdHistSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    macdSignals.Add(macdValues[i]);

                    macdHistSignals.Add(macdHistValues[i]);
                }

                return (macdSignals.ToArray(), macdHistSignals.ToArray()); // Return arrays of MACD and Histogram signals
            }
            else
            {
                throw new Exception($"Error calculating MACD (Ext) indicator. RetCode: {retCode}");
            }
        }


        public float[] Macd(List<ITradingDataPoint<T>> fxData, int optInSignalPeriod = 9)
        {
            return MacdFix(fxData, optInSignalPeriod).Macd;
        }
        public float[] MacdFixMacdFixSignals(List<ITradingDataPoint<T>> fxData, int optInSignalPeriod = 9)
        {
            return MacdFix(fxData, optInSignalPeriod).MacdFixSignals;
        }
        public float[] MacdFixMacdFixHistSignals(List<ITradingDataPoint<T>> fxData, int optInSignalPeriod = 9)
        {
            return MacdFix(fxData, optInSignalPeriod).MacdFixHistSignals;
        }
        private (float[] Macd, float[] MacdFixSignals, float[] MacdFixHistSignals) MacdFix(List<ITradingDataPoint<T>> fxData, int optInSignalPeriod = 9)
        {
            int lookback = Functions.MacdFixLookback(optInSignalPeriod);

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInSignalPeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var macdValues = new float[NoReturned];
            var macdSignalValues = new float[NoReturned];
            var macdHistValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MACD Fix function
            var retCode = Functions.MacdFix<float>(
                prices,
                range,
                macdValues,
                macdSignalValues,
                macdHistValues,
                out var outputRange,
                optInSignalPeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (macdValues.ToArray(), macdSignalValues.ToArray(), macdHistValues.ToArray()); // Return arrays of MACD and Histogram signals
            }
            else
            {
                throw new Exception($"Error calculating MACD (Fix) indicator. RetCode: {retCode}");
            }
        }


        public float[] MamaMamaSignals(List<ITradingDataPoint<T>> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            return Mama(fxData, optInFastLimit, optInSlowLimit).MamaSignals;
        }
        public float[] MamaFamaSignals(List<ITradingDataPoint<T>> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            return Mama(fxData, optInFastLimit, optInSlowLimit).FamaSignals;
        }
        private (float[] MamaSignals, float[] FamaSignals) Mama(List<ITradingDataPoint<T>> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            int lookback = Functions.MamaLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period: {optInFastLimit} and Slow Period:{optInSlowLimit}.");
            }


            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var mamaValues = new float[NoReturned];
            var famaValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MAMA function
            var retCode = Functions.Mama<float>(
                prices,
                range,
                mamaValues,
                famaValues,
                out var outputRange,
                optInFastLimit,
                optInSlowLimit);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return (mamaValues.ToArray(), famaValues.ToArray()); // Return arrays of MAMA and FAMA signals
            }
            else
            {
                throw new Exception($"Error calculating MAMA indicator. RetCode: {retCode}");
            }
        }

        public float[] Max(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.MaxLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var maxValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call Max function
            var retCode = Functions.Max<float>(
                prices,
                range,
                maxValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return maxValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Max indicator. RetCode: {retCode}");
            }
        }
        public float[] MaxIndex(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.MaxIndexLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var maxIndices = new int[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MaxIndex function
            var retCode = Functions.MaxIndex<float>(
                prices,
                range,
                maxIndices,
                out var outputRange,
                optInTimePeriod);

            // If successful, extract and return indices
            if (retCode == Core.RetCode.Success)
            {
                return maxIndices.Select(r=>(float)r).ToArray(); // Return the array of indices
            }
            else
            {
                throw new Exception($"Error calculating MaxIndex indicator. RetCode: {retCode}");
            }
        }
        public float[] MedPrice(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.MedPriceLookback();
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var medPrices = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call MedPrice function
            var retCode = Functions.MedPrice<float>(
                highs,
                lows,
                range,
                medPrices,
                out var outputRange);

            // If successful, normalize and return MedPrice values
            if (retCode == Core.RetCode.Success)
            {
                return medPrices.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MedPrice indicator. RetCode: {retCode}");
            }
        }

        public float[] MidPoint(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.MidPointLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var midPoints = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MidPoint function
            var retCode = Functions.MidPoint<float>(
                prices,
                range,
                midPoints,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return MidPoint values
            if (retCode == Core.RetCode.Success)
            {
                return midPoints.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MidPoint indicator. RetCode: {retCode}");
            }
        }
        public float[] MidPrice(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.MidPriceLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var midPrices = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call MidPrice function
            var retCode = Functions.MidPrice<float>(
                highs,
                lows,
                range,
                midPrices,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return MidPrice values
            if (retCode == Core.RetCode.Success)
            {
                return midPrices.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MidPrice indicator. RetCode: {retCode}");
            }
        }
        public float[] Min(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)

        {
            int lookback = Functions.MinLookback(optInTimePeriod);

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }


            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            
            var minValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call Min function
            var retCode = Functions.Min<float>(
                prices,
                range,
                minValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Min values
            if (retCode == Core.RetCode.Success)
            {
                return minValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Min indicator. RetCode: {retCode}");
            }
        }
        public float[] MinIndex(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.MinIndexLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var minIndices = new int[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MinIndex function
            var retCode = Functions.MinIndex<float>(
                prices,
                range,
                minIndices,
                out var outputRange,
                optInTimePeriod);

            // If successful, extract and return indices
            if (retCode == Core.RetCode.Success)
            {
                return minIndices.Select(r=>(float)r).ToArray(); // Return the array of indices
            }
            else
            {
                throw new Exception($"Error calculating MinIndex indicator. RetCode: {retCode}");
            }
        }
        public float[] MinMaxIndex(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.MinMaxIndexLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var minIndices = new float[NoReturned];
            var maxIndices = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call MinMaxIndex function
            var retCode = Functions.MinMaxIndex<float>(
                prices,
                range,
                minIndices,
                maxIndices,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return MinMaxIndex values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                int x = 0;
                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(maxIndices[x] - minIndices[x]);
                    x++;
                }

                return signals.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MinMaxIndex indicator. RetCode: {retCode}");
            }
        }
        public float[] MinusDI(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.MinusDILookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var minusDIValues = new float[NoReturned];
            var range = new Range(0, highs.Length-1);

            // Call MinusDI function
            var retCode = Functions.MinusDI<float>(
                highs,
                lows,
                closes,
                range,
                minusDIValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return MinusDI values
            if (retCode == Core.RetCode.Success)
            {
                return minusDIValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MinusDI indicator. RetCode: {retCode}");
            }
        }
        public float[] MinusDM(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.MinusDMLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var minusDMValues = new float[NoReturned];
            var range = new Range(0, highs.Length -1);

            // Call MinusDM function
            var retCode = Functions.MinusDM<float>(
                highs,
                lows,
                range,
                minusDMValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return MinusDM values
            if (retCode == Core.RetCode.Success)
            {
                return minusDMValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MinusDM indicator. RetCode: {retCode}");
            }
        }
        public float[] Mom(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 10)
        {
            int lookback = Functions.MomLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var momValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call Mom function
            var retCode = Functions.Mom<float>(
                prices,
                range,
                momValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Mom values
            if (retCode == Core.RetCode.Success)
            {
                return momValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Mom indicator. RetCode: {retCode}");
            }
        }
        public float[] Mult(List<ITradingDataPoint<T>> fxData1, List<ITradingDataPoint<T>> fxData2)
        {
            int lookback = Functions.MultLookback();
            
            if (fxData1 == null || fxData1.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            if (fxData1 == null || fxData2 == null || fxData1.Count != fxData2.Count)
            {
                throw new Exception("Both data lists must have the same number of elements to calculate the Mult indicator.");
            }

            // Extract values as floats (using Close prices or any other relevant value)
            var values1 = fxData1.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var values2 = fxData2.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData1.Count() - lookback;
            
            var multValues = new float[NoReturned];
            var range = new Range(0, values1.Length - 1);

            // Call Mult function
            var retCode = Functions.Mult<float>(
                values1,
                values2,
                range,
                multValues,
                out var outputRange);

            // If successful, normalize and return Mult values
            if (retCode == Core.RetCode.Success)
            {
                return multValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Mult indicator. RetCode: {retCode}");
            }
        }
        public float[] Natr(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.NatrLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var natrValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call NATR function
            var retCode = Functions.Natr<float>(
                highs,
                lows,
                closes,
                range,
                natrValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return NATR values
            if (retCode == Core.RetCode.Success)
            {
                return natrValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating NATR indicator. RetCode: {retCode}");
            }
        }
        public float[] PlusDI(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.PlusDILookback(optInTimePeriod);

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }


            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var plusDIValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call PlusDI function
            var retCode = Functions.PlusDI<float>(
                highs,
                lows,
                closes,
                range,
                plusDIValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return PlusDI values
            if (retCode == Core.RetCode.Success)
            {
                return plusDIValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating PlusDI indicator. RetCode: {retCode}");
            }
        }
        public float[] PlusDM(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.PlusDMLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            
            float[] plusDMValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call PlusDM function
            var retCode = Functions.PlusDM<float>(
                highs,
                lows,
                range,
                plusDMValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return PlusDM values
            if (retCode == Core.RetCode.Success)
            {
                return plusDMValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating PlusDM indicator. RetCode: {retCode}");
            }
        }
        public float[] Ppo(List<ITradingDataPoint<T>> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma)
        {
            int lookback = Functions.PpoLookback(optInFastPeriod, optInSlowPeriod, optInMAType);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{optInFastPeriod} and Slow Period: {optInSlowPeriod}");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            
            var ppoValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call PPO function
            var retCode = Functions.Ppo<float>(
                prices,
                range,
                ppoValues,
                out var outputRange,
                optInFastPeriod,
                optInSlowPeriod,
                optInMAType);

            // If successful, normalize and return PPO values
            if (retCode == Core.RetCode.Success)
            {
                return ppoValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating PPO indicator. RetCode: {retCode}");
            }
        }
        public float[] Roc(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 10)
        {
            int lookback = Functions.RocLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var rocValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call ROC function
            var retCode = Functions.Roc<float>(
                prices,
                range,
                rocValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return ROC values
            if (retCode == Core.RetCode.Success)
            {
                return rocValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ROC indicator. RetCode: {retCode}");
            }
        }
        public float[] RocP(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 10)
        {
            int lookback = Functions.RocPLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var rocpValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call ROCP function
            var retCode = Functions.RocP<float>(
                prices,
                range,
                rocpValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return ROCP values
            if (retCode == Core.RetCode.Success)
            {
                return rocpValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ROCP indicator. RetCode: {retCode}");
            }
        }
        public float[] RocR(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 10)
        {
            int lookback = Functions.RocRLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var rocrValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call ROCR function
            var retCode = Functions.RocR<float>(
                prices,
                range,
                rocrValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return ROCR values
            if (retCode == Core.RetCode.Success)
            {
                return rocrValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ROCR indicator. RetCode: {retCode}");
            }
        }
        public float[] RocR100(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 10)
        {
            int lookback = Functions.RocR100Lookback(optInTimePeriod);

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var rocr100Values = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call ROCR100 function
            var retCode = Functions.RocR100<float>(
                prices,
                range,
                rocr100Values,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return ROCR100 values
            if (retCode == Core.RetCode.Success)
            {
                return rocr100Values.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating ROCR100 indicator. RetCode: {retCode}");
            }
        }
        public float[] Rsi(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookback = Functions.RsiLookback(optInTimePeriod);

            
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            
            var rsiValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call RSI function
            var retCode = Functions.Rsi<float>(
                prices,
                range,
                rsiValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return RSI values
            if (retCode == Core.RetCode.Success)
            {
                return rsiValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating RSI indicator. RetCode: {retCode}");
            }
        }
        public float[] Sar(List<ITradingDataPoint<T>> fxData, double optInAcceleration = 0.02, double optInMaximum = 0.2)
        {
            int lookback = Functions.SarLookback();


            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Acceleration:{optInAcceleration} and OptIn Maximum:{optInMaximum}");
            }

            // Extract high and low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var sarValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call SAR function
            var retCode = Functions.Sar<float>(
                highs,
                lows,
                range,
                sarValues,
                out var outputRange,
                optInAcceleration,
                optInMaximum);

            // If successful, normalize and return SAR values
            if (retCode == Core.RetCode.Success)
            {
                return sarValues.ToArray(); // Return the list of signals
            }
            else
            {
                throw new Exception($"Error calculating SAR indicator. RetCode: {retCode}");
            }
        }
        public float[] SarExt(List<ITradingDataPoint<T>> fxData, double optInStartValue = 0.0, double optInOffsetOnReverse = 0.0, double optInAccelerationInitLong = 0.02, double optInAccelerationLong = 0.02, double optInAccelerationMaxLong = 0.2, double optInAccelerationInitShort = 0.02, double optInAccelerationShort = 0.02, double optInAccelerationMaxShort = 0.2)
        {
            int lookback = Functions.SarExtLookback();


            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high and low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;

            var sarValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call SAR Ext function
            var retCode = Functions.SarExt<float>(
                highs,
                lows,
                range,
                sarValues,
                out var outputRange,
                optInStartValue,
                optInOffsetOnReverse,
                optInAccelerationInitLong,
                optInAccelerationLong,
                optInAccelerationMaxLong,
                optInAccelerationInitShort,
                optInAccelerationShort,
                optInAccelerationMaxShort);

            // If successful, normalize and return SAR Ext values
            if (retCode == Core.RetCode.Success)
            {
                return sarValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating SAR Ext indicator. RetCode: {retCode}");
            }
        }
        public float[] Sin(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.SinLookback();

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var sinValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Sin function
            var retCode = Functions.Sin<float>(
                inputValues,
                range,
                sinValues,
                out var outputRange);

            // If successful, normalize and return Sin values
            if (retCode == Core.RetCode.Success)
            {
                return sinValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Sin indicator. RetCode: {retCode}");
            }
        }
        public float[] Sinh(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.SinhLookback();
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault()) - Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var sinhValues = new float[NoReturned];
            var range = new Range(0, PriceChanges.Length - 1);

            // Call Sinh function
            var retCode = Functions.Sinh<float>(
                PriceChanges,
                range,
                sinhValues,
                out var outputRange);

            // If successful, normalize and return Sinh values
            if (retCode == Core.RetCode.Success)
            {
                return sinhValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Sinh indicator. RetCode: {retCode}");
            }
        }
        public float[] Sma(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.SmaLookback(optInTimePeriod);

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            var smaValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call SMA function
            var retCode = Functions.Sma<float>(
                inputValues,
                range,
                smaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return SMA values
            if (retCode == Core.RetCode.Success)
            {
                return smaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating SMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Sqrt(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.SqrtLookback();

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            int NoReturned = fxData.Count() - lookback;
            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            var sqrtValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Sqrt function
            var retCode = Functions.Sqrt<float>(
                inputValues,
                range,
                sqrtValues,
                out var outputRange);

            // If successful, normalize and return Sqrt values
            if (retCode == Core.RetCode.Success)
            {
                return sqrtValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Sqrt indicator. RetCode: {retCode}");
            }
        }
        public float[] StdDev(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 5, double optInNbDev = 1.0)
        {
            int lookback = Functions.StdDevLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var stdDevValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call StdDev function
            var retCode = Functions.StdDev<float>(
                inputValues,
                range,
                stdDevValues,
                out var outputRange,
                optInTimePeriod,
                optInNbDev);

            // If successful, normalize and return StdDev values
            if (retCode == Core.RetCode.Success)
            {
                return stdDevValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating StdDev indicator. RetCode: {retCode}");
            }
        }
        public float[] Stoch(List<ITradingDataPoint<T>> fxData, int optInFastKPeriod = 5, int optInSlowKPeriod = 3, Core.MAType optInSlowKMAType = Core.MAType.Sma, int optInSlowDPeriod = 3, Core.MAType optInSlowDMAType = Core.MAType.Sma)
        {
            int lookback = Functions.StochLookback(optInFastKPeriod, optInSlowKPeriod, optInSlowKMAType, optInSlowDPeriod, optInSlowDMAType);


            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;

            var slowKValues = new float[NoReturned];
            var slowDValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call Stoch function
            var retCode = Functions.Stoch<float>(
                highs,
                lows,
                closes,
                range,
                slowKValues,
                slowDValues,
                out var outputRange,
                optInFastKPeriod,
                optInSlowKPeriod,
                optInSlowKMAType,
                optInSlowDPeriod,
                optInSlowDMAType);

            // If successful, normalize and return Stochastic values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();
                int x = 0;
                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(slowKValues[x] + slowDValues[x]);
                    x++;
                }

                return signals.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Stochastic indicator. RetCode: {retCode}");
            }
        }
        public float[] StochF(List<ITradingDataPoint<T>> fxData, int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma)
        {
            int lookback = Functions.StochFLookback(optInFastKPeriod, optInFastDPeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast K Period: {optInFastKPeriod} and Fast D Period: {optInFastDPeriod}");
            }


            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var fastKValues = new float[NoReturned];
            var fastDValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call StochF function
            var retCode = Functions.StochF<float>(
                highs,
                lows,
                closes,
                range,
                fastKValues,
                fastDValues,
                out var outputRange,
                optInFastKPeriod,
                optInFastDPeriod,
                optInFastDMAType);

            // If successful, normalize and return Stochastic Fast values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();
                int x = 0;
                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(fastKValues[x] + fastDValues[x]);
                    x++;
                }

                return signals.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Stochastic Fast indicator. RetCode: {retCode}");
            }
        }
        public float[] StochRsi(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14, int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma)
        {
            int lookback = Functions.StochRsiLookback(optInFastKPeriod, optInFastDPeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast K Period: {optInFastKPeriod} and Fast D Period: {optInFastDPeriod}");
            }

            // Extract close values as floats
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var fastKValues = new float[NoReturned];
            var fastDValues = new float[NoReturned];
            var range = new Range(0, closes.Length - 1);

            // Call StochRsi function
            var retCode = Functions.StochRsi<float>(
                closes,
                range,
                fastKValues,
                fastDValues,
                out var outputRange,
                optInTimePeriod,
                optInFastKPeriod,
                optInFastDPeriod,
                optInFastDMAType);

            // If successful, normalize and return Stochastic RSI values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();
                int x = 0;
                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(fastKValues[x] + fastDValues[x]);
                    x++;
                }

                return signals.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Stochastic RSI indicator. RetCode: {retCode}");
            }
        }
        public float[] Sub(List<ITradingDataPoint<T>> fxData0, List<ITradingDataPoint<T>> fxData1)
        {
            int lookback = Functions.SubLookback();
            if (fxData0 == null || fxData0.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract input values as floats
            var inputValues0 = fxData0.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var inputValues1 = fxData1.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData0.Count() - lookback;

            var subValues = new float[NoReturned];
            var range = new Range(0, inputValues0.Length - 1);

            // Call Sub function
            var retCode = Functions.Sub<float>(
                inputValues0,
                inputValues1,
                range,
                subValues,
                out var outputRange);

            // If successful, normalize and return Sub values
            if (retCode == Core.RetCode.Success)
            {
                return subValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Sub indicator. RetCode: {retCode}");
            }
        }
        public float[] Sum(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.SumLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;

            var sumValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Sum function
            var retCode = Functions.Sum<float>(
                inputValues,
                range,
                sumValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Sum values
            if (retCode == Core.RetCode.Success)
            {
                return sumValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Sum indicator. RetCode: {retCode}");
            }
        }
        public float[] T3(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 5, double optInVFactor = 0.7)
        {
            int lookback = Functions.T3Lookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookback;


            var t3Values = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call T3 function
            var retCode = Functions.T3<float>(
                inputValues,
                range,
                t3Values,
                out var outputRange,
                optInTimePeriod,
                optInVFactor);

            if (retCode == Core.RetCode.Success)
            {
                return t3Values.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating T3 indicator. RetCode: {retCode}");
            }
        }
        public float[] Tan(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.TanLookback();
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            var tanValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Tan function
            var retCode = Functions.Tan<float>(
                inputValues,
                range,
                tanValues,
                out var outputRange);

            // If successful, normalize and return Tan values
            if (retCode == Core.RetCode.Success)
            {
                return tanValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Tan indicator. RetCode: {retCode}");
            }
        }
        public float[] Tanh(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.TanhLookback();
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            var PriceChanges = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault()) - Convert.ToSingle(d.Open.GetValueOrDefault())).ToArray().NormalizeToRange().AsSpan();

            int NoReturned = fxData.Count() - lookback;
            var tanhValues = new float[NoReturned];
            var range = new Range(0, PriceChanges.Length - 1);

            // Call Tanh function
            var retCode = Functions.Tanh<float>(
                PriceChanges,
                range,
                tanhValues,
                out var outputRange);

            // If successful, normalize and return Tanh values
            if (retCode == Core.RetCode.Success)
            {
                return tanhValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Tanh indicator. RetCode: {retCode}");
            }
        }
        public float[] Tema(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {

            int lookback = Functions.TemaLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            var temaValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Tema function
            var retCode = Functions.Tema<float>(
                inputValues,
                range,
                temaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return TEMA values
            if (retCode == Core.RetCode.Success)
            {
                return temaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating TEMA indicator. RetCode: {retCode}");
            }
        }
        public float[] TRange(List<ITradingDataPoint<T>> fxData)
        {
            int lookback = Functions.TRangeLookback();
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            var tRangeValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call TRange function
            var retCode = Functions.TRange<float>(
                highs,
                lows,
                closes,
                range,
                tRangeValues,
                out var outputRange);

            // If successful, normalize and return True Range values
            if (retCode == Core.RetCode.Success)
            {
                return tRangeValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating True Range indicator. RetCode: {retCode}");
            }
        }
        public float[] Trima(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            int lookback = Functions.TrimaLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            var trimaValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Trima function
            var retCode = Functions.Trima<float>(
                inputValues,
                range,
                trimaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Trima values
            if (retCode == Core.RetCode.Success)
            {
                return trimaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Trima indicator. RetCode: {retCode}");
            }
        }
        public float[] Trix(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {

            int lookBack = Functions.TrixLookback(optInTimePeriod);
            
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookBack;
            var trixValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Trix function
            var retCode = Functions.Trix<float>(
                inputValues,
                range,
                trixValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Trix values
            if (retCode == Core.RetCode.Success)
            {
                return trixValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Trix indicator. RetCode: {retCode}");
            }
        }
        public float[] Tsf(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookBack = Functions.TsfLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookBack;

            var tsfValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Tsf function
            var retCode = Functions.Tsf<float>(
                inputValues,
                range,
                tsfValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return TSF values
            if (retCode == Core.RetCode.Success)
            {
                return tsfValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating TSF indicator. RetCode: {retCode}");
            }
        }
        public float[] TypPrice(List<ITradingDataPoint<T>> fxData)
        {
            int lookBack = Functions.TypPriceLookback();

            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookBack;

            var typPriceValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call TypPrice function
            var retCode = Functions.TypPrice<float>(
                highs,
                lows,
                closes,
                range,
                typPriceValues,
                out var outputRange);

            // If successful, normalize and return Typical Price values
            if (retCode == Core.RetCode.Success)
            {
                return typPriceValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Typical Price indicator. RetCode: {retCode}");
            }
        }
        public float[] UltOsc(List<ITradingDataPoint<T>> fxData, int optInTimePeriod1 = 7, int optInTimePeriod2 = 14, int optInTimePeriod3 = 28)
        {
            int lookback = Functions.UltOscLookback(optInTimePeriod1, optInTimePeriod1, optInTimePeriod3);

            if (fxData == null || fxData.Count < lookback)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Period1:{optInTimePeriod1} and Period2:{optInTimePeriod2} and Period3:{optInTimePeriod3}");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookback;
            var ultOscValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call UltOsc function
            var retCode = Functions.UltOsc<float>(
                highs,
                lows,
                closes,
                range,
                ultOscValues,
                out var outputRange,
                optInTimePeriod1,
                optInTimePeriod2,
                optInTimePeriod3);

            // If successful, normalize and return Ultimate Oscillator values
            if (retCode == Core.RetCode.Success)
            {
                return ultOscValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Ultimate Oscillator. RetCode: {retCode}");
            }
        }
        public float[] Var(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 5)
        {
            int lookBack = Functions.VarLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookBack;
            var varValues = new float[NoReturned];
            var range = new Range(0, inputValues.Length - 1);

            // Call Var function
            var retCode = Functions.Var<float>(
                inputValues,
                range,
                varValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Variance values
            if (retCode == Core.RetCode.Success)
            {
                return varValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Variance indicator. RetCode: {retCode}");
            }
        }
        public float[] WclPrice(List<ITradingDataPoint<T>> fxData)
        {
            int lookBack = Functions.VarLookback();

            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            var wclPriceValues = new float[fxData.Count];
            var range = new Range(0, highs.Length - 1);

            // Call WclPrice function
            var retCode = Functions.WclPrice<float>(
                highs,
                lows,
                closes,
                range,
                wclPriceValues,
                out var outputRange);

            // If successful, normalize and return Weighted Close Price values
            if (retCode == Core.RetCode.Success)
            {
                return wclPriceValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Weighted Close Price indicator. RetCode: {retCode}");
            }
        }
        public float[] WillR(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {
            int lookBack = Functions.WillRLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookBack;
            var willRValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call WillR function
            var retCode = Functions.WillR<float>(
                highs,
                lows,
                closes,
                range,
                willRValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return Williams %R values
            if (retCode == Core.RetCode.Success)
            {
                return willRValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating Williams %R indicator. RetCode: {retCode}");
            }
        }
        public float[] Wma(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 30)
        {
            
            int lookBack = Functions.WmaLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract the real values (e.g., close prices) as floats
            var realValues = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();


            int NoReturned = fxData.Count() - lookBack;
            var wmaValues = new float[NoReturned];
            var range = new Range(0, realValues.Length - 1);

            // Call Wma function
            var retCode = Functions.Wma<float>(
                realValues,
                range,
                wmaValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return WMA values
            if (retCode == Core.RetCode.Success)
            {
                return wmaValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating WMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Obv(List<ITradingDataPoint<T>> fxData)
        {

            int lookBack = Functions.ObvLookback();
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices and volumes as floats
            var prices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookBack;

            var obvValues = new float[NoReturned];
            var range = new Range(0, prices.Length-1);

            // Call OBV function
            var retCode = Functions.Obv<float>(
                prices,
                volumes,
                range,
                obvValues,
                out var outputRange);

            // If successful, normalize and return OBV values
            if (retCode == Core.RetCode.Success)
            {
                return obvValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating OBV indicator. RetCode: {retCode}");
            }
        }
        public float[] AdOsc(List<ITradingDataPoint<T>> fxData, int fastPeriod = 3, int slowPeriod = 10)
        {

            int lookBack = Functions.AdOscLookback(fastPeriod, slowPeriod);
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{fastPeriod} and Slow Period:{slowPeriod}");
            }

            // Extract high, low, close prices, and volume as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume.GetValueOrDefault())).ToArray().AsSpan(); // Assuming ITradingDataPoint<T> has a Volume property

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length || closePrices.Length != volumes.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }


            int NoReturned = fxData.Count() - lookBack;
            var adOscValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call AdOsc function
            var retCode = Functions.AdOsc<float>(
                highPrices,
                lowPrices,
                closePrices,
                volumes,
                range,
                adOscValues,
                out var outputRange,
                fastPeriod,
                slowPeriod);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return adOscValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating AD Oscillator. RetCode: {retCode}");
            }
        }
        public float[] Mfi(List<ITradingDataPoint<T>> fxData, int optInTimePeriod = 14)
        {

            int lookBack = Functions.MfiLookback(optInTimePeriod);
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, Close, and Volume as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume.GetValueOrDefault())).ToArray().AsSpan();

            int NoReturned = fxData.Count() - lookBack;
            var mfiValues = new float[NoReturned];
            var range = new Range(0, highs.Length - 1);

            // Call MFI function
            var retCode = Functions.Mfi<float>(
                highs,
                lows,
                closes,
                volumes,
                range,
                mfiValues,
                out var outputRange,
                optInTimePeriod);

            // If successful, normalize and return MFI values
            if (retCode == Core.RetCode.Success)
            {
                return mfiValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating MFI indicator. RetCode: {retCode}");
            }
        }
        public float[] Ad(List<ITradingDataPoint<T>> fxData)
        {

            int lookBack = Functions.AdLookback();
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, close prices, and volume as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High.GetValueOrDefault())).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low.GetValueOrDefault())).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close.GetValueOrDefault())).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume.GetValueOrDefault())).ToArray().AsSpan(); // Assuming ITradingDataPoint<T> has a Volume property

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length || closePrices.Length != volumes.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }


            int NoReturned = fxData.Count() - lookBack;
            var adValues = new float[NoReturned];
            var range = new Range(0, highPrices.Length - 1);

            // Call Ad function
            var retCode = Functions.Ad<float>(
                highPrices,
                lowPrices,
                closePrices,
                volumes,
                range,
                adValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                return adValues.ToArray(); 
            }
            else
            {
                throw new Exception($"Error calculating AD indicator. RetCode: {retCode}");
            }
        }

    }
}
