using System.Reflection;
using CallaghanDev.Finance.TechnicalAnalysis.Exceptions;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public class Indicators
    {
        public float[] Accbands(List<IndicatorDataPoint> fxData, int optinTimePeriod = 20)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AccbandsLookback(optinTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({optinTimePeriod}).");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output arrays
            var upperBand = new float[fxData.Count];
            var middleBand = new float[fxData.Count];
            var lowerBand = new float[fxData.Count];

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

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    if (closePrices[i] > upperBand[i])
                    {
                        signals.Add(1);
                    }
                    else if (closePrices[i] < lowerBand[i])
                    {
                        signals.Add(-1);
                    }
                    else
                    {
                        signals.Add(0);
                    }
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating acceleration bands. RetCode: {retCode}");
            }

        }
        public float[] Acos(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AcosLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (closePrices.Length < 1)
            {
                throw new Exception("Invalid close price data.");
            }

            // Prepare output arrays
            var acosValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length-1);

            // Call Acos function
            var retCode = Functions.Acos<float>(
                closePrices,
                range,
                acosValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var acosValue = acosValues[i];

                    signals.Add(acosValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ACos indicator. RetCode: {retCode}");
            }
        }
        public float[] Add(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AcosLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator.");
            }

            // Extract two sets of values to be added, e.g., Bid and Offer
            var bidPrices = fxData.Select(d => Convert.ToSingle(d.Bid ?? 0m)).ToArray().AsSpan();
            var offerPrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (bidPrices.Length != offerPrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var addedValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var addedValue = addedValues[i];

                    signals.Add(addedValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Add indicator. RetCode: {retCode}");
            }
        }
        public float[] Adx(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AdxLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }
            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var adxValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adxValue = adxValues[i];

                    signals.Add(adxValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ADX indicator. RetCode: {retCode}");
            }
        }
        public float[] Adxr(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AdxrLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var adxrValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adxrValue = adxrValues[i];

                    signals.Add(adxrValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ADXR indicator. RetCode: {retCode}");
            }
        }
        public float[] Apo(List<IndicatorDataPoint> fxData, int fastPeriod = 12, int slowPeriod = 26, Core.MAType maType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.ApoLookback(fastPeriod, slowPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen fast time period ({fastPeriod}) and the chosen slow time period {slowPeriod}.");
            }
            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var apoValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var apoValue = apoValues[i];


                    signals.Add(apoValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating APO indicator. RetCode: {retCode}");
            }
        }
        public float[] AroonUp(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            return Aroon(fxData, timePeriod).aroonUp;
        }
        public float[] AroonDown(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            return Aroon(fxData, timePeriod).aroonUp;
        }
        private (float[] aroonUp, float[] aroonDown) Aroon(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AroonLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }
            // Extract high and low prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output arrays
            var aroonUpValues = new float[fxData.Count];
            var aroonDownValues = new float[fxData.Count];
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
                var aroonUpSignals = new List<float>();
                var aroonDownSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    aroonUpSignals.Add(aroonUpValues[i]);
                    aroonDownSignals.Add(aroonDownValues[i]);
                }

                return (aroonUpSignals.ToArray(), aroonDownSignals.ToArray()); // Return both arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating Aroon indicator. RetCode: {retCode}");
            }
        }
        public float[] AroonOsc(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AroonOscLookback(timePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period ({timePeriod}).");
            }

            // Extract high and low prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var aroonOscValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var aroonOscValue = aroonOscValues[i];

                    signals.Add(aroonOscValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Aroon Oscillator. RetCode: {retCode}");
            }
        }
        public float[] Asin(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AsinLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var asinValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length - 1);

            // Call Asin function
            var retCode = Functions.Asin<float>(
                closePrices,
                range,
                asinValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var asinValue = asinValues[i];


                    signals.Add(asinValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ASIN indicator. RetCode: {retCode}");
            }
        }
        public float[] Atan(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AtanLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var atanValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length - 1);

            // Call Atan function
            var retCode = Functions.Atan<float>(
                closePrices,
                range,
                atanValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var atanValue = atanValues[i];


                    signals.Add(atanValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ATAN indicator. RetCode: {retCode}");
            }
        }
        public float[] Atr(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.AtrLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var atrValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var atrValue = atrValues[i];

                    signals.Add(atrValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ATR indicator. RetCode: {retCode}");
            }
        }
        public float[] AvgDev(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AvgDevLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var avgDevValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var avgDevValue = avgDevValues[i];

                    signals.Add(avgDevValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Average Deviation indicator. RetCode: {retCode}");
            }
        }
        public float[] AvgPrice(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.AvgPriceLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract open, high, low, and close prices as floats
            var openPrices = fxData.Select(d => Convert.ToSingle(d.MidOpen ?? 0m)).ToArray().AsSpan();
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Validate lengths
            if (openPrices.Length != highPrices.Length || highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var avgPriceValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var avgPriceValue = avgPriceValues[i];

                    signals.Add(avgPriceValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Average Price indicator. RetCode: {retCode}");
            }
        }

        public float[] BbandsUpperBandSignals(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return Bbands(fxData, timePeriod, nbDevUp, nbDevDn, maType).upperBandSignals;
        }
        public float[] BbandsMiddleBandSignals(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return Bbands(fxData, timePeriod, nbDevUp, nbDevDn, maType).middleBandSignals;
        }
        public float[] BbandsLowerBandSignals(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return Bbands(fxData, timePeriod, nbDevUp, nbDevDn, maType).lowerBandSignals;
        }
        private (float[] upperBandSignals, float[] middleBandSignals, float[] lowerBandSignals) Bbands(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.BbandsLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var upperBandValues = new float[fxData.Count];
            var middleBandValues = new float[fxData.Count];
            var lowerBandValues = new float[fxData.Count];
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
                var upperBandSignals = new List<float>();
                var middleBandSignals = new List<float>();
                var lowerBandSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    upperBandSignals.Add(upperBandValues[i]);
                    middleBandSignals.Add(middleBandValues[i]);
                    lowerBandSignals.Add(lowerBandValues[i]);
                }

                return (upperBandSignals.ToArray(), middleBandSignals.ToArray(), lowerBandSignals.ToArray());
            }
            else
            {
                throw new Exception($"Error calculating Bollinger Bands. RetCode: {retCode}");
            }
        }

        public float[] Beta(List<IndicatorDataPoint> fxData1, List<IndicatorDataPoint> fxData2, int timePeriod = 5)
        {
            // Validate input data length
            if (fxData1 == null || fxData1.Count < Functions.BetaLookback() ||fxData2.Count < Functions.BetaLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            if (fxData1.Count != fxData2.Count)
            {
                throw new Exception("Input data lengths must match.");
            }

            // Extract close prices from both data sets
            var closePrices1 = fxData1.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var closePrices2 = fxData2.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var betaValues = new float[fxData1.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var betaValue = betaValues[i];


                    signals.Add(betaValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Beta indicator. RetCode: {retCode}");
            }
        }
        public float[] Bop(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.BopLookback() )
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract open, high, low, and close prices as floats
            var openPrices = fxData.Select(d => Convert.ToSingle(d.MidOpen ?? 0m)).ToArray().AsSpan();
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var bopValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var bopValue = bopValues[i];

                    signals.Add(bopValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Balance of Power indicator. RetCode: {retCode}");
            }
        }
        public float[] Cci(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.CciLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var cciValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var cciValue = cciValues[i];
                    signals.Add(cciValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating CCI indicator. RetCode: {retCode}");
            }
        }
        public float[] Ceil(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.CeilLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var ceilValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var ceilValue = ceilValues[i];
                    signals.Add(ceilValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Ceiling indicator. RetCode: {retCode}");
            }
        }
        public float[] Cmo(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.CmoLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var cmoValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var cmoValue = cmoValues[i];

                    signals.Add(cmoValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating CMO indicator. RetCode: {retCode}");
            }
        }
        public float[] Correl(List<IndicatorDataPoint> fxData1, List<IndicatorDataPoint> fxData2, int timePeriod = 30)
        {
            // Validate input data length
            if (fxData1 == null || fxData2 == null || fxData1.Count < Functions.CorrelLookback() || fxData2.Count < Functions.CorrelLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {timePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices1 = fxData1.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var prices2 = fxData2.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var correlValues = new float[fxData1.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var correlValue = correlValues[i];

                    signals.Add(correlValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating CORREL indicator. RetCode: {retCode}");
            }
        }
        public float[] Cos(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.CosLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var cosValues = new float[fxData.Count];
            var range = new Range(0, prices.Length - 1);

            // Call Cos function
            var retCode = Functions.Cos<float>(
                prices,
                range,
                cosValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var cosValue = cosValues[i];

                    signals.Add(cosValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating COS indicator. RetCode: {retCode}");
            }
        }
        public float[] Cosh(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.CoshLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var coshValues = new float[fxData.Count];
            var range = new Range(0, prices.Length - 1);

            // Call Cosh function
            var retCode = Functions.Cosh<float>(
                prices,
                range,
                coshValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var coshValue = coshValues[i];

                    signals.Add(coshValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating COSH indicator. RetCode: {retCode}");
            }
        }
        public float[] Dema(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.DemaLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }


            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var demaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var demaValue = demaValues[i];
                    signals.Add(demaValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating DEMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Div(List<IndicatorDataPoint> fxData, List<IndicatorDataPoint> fxData2)
        {
            // Validate input data lengths
            if (fxData == null || fxData2 == null || fxData.Count < Functions.DivLookback() || fxData2.Count < Functions.DivLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            if (fxData == null || fxData2 == null || fxData.Count != fxData2.Count || fxData.Count == 0)
            {
                throw new Exception("Input data lists must have the same non-zero length.");
            }

            // Extract prices from fxData and fxData2 (using Offer or Bid values)
            var prices1 = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var prices2 = fxData2.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var divValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var divValue = divValues[i];

                    signals.Add(divValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Div indicator. RetCode: {retCode}");
            }
        }
        public float[] Dx(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null ||  fxData.Count < Functions.DxLookback(optInTimePeriod) )
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, and Close prices
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close)).ToArray().AsSpan();

            // Prepare output array
            var dxValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var dxValue = dxValues[i];

                    signals.Add(dxValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating DX indicator. RetCode: {retCode}");
            }
        }
        public float[] Ema(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.EmaLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var emaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var emaValue = emaValues[i];

                    signals.Add(emaValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating EMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Exp(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.ExpLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var expValues = new float[fxData.Count];
            var range = new Range(0, prices.Length - 1);

            // Call Exp function
            var retCode = Functions.Exp<float>(
                prices,
                range,
                expValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var expValue = expValues[i];

                    signals.Add(expValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Exp indicator. RetCode: {retCode}");
            }
        }
        public float[] Floor(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.FloorLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var floorValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var floorValue = floorValues[i];

                    signals.Add(floorValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Floor indicator. RetCode: {retCode}");
            }
        }
        public float[] HtDcPeriod(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.HtDcPeriodLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var htDcPeriodValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var htDcPeriodValue = htDcPeriodValues[i];

                    signals.Add(htDcPeriodValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating HT DC Period indicator. RetCode: {retCode}");
            }
        }
        public float[] HtDcPhase(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.HtDcPhaseLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var htDcPhaseValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var htDcPhaseValue = htDcPhaseValues[i];

                    signals.Add(htDcPhaseValue);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating HT DC Phase indicator. RetCode: {retCode}");
            }
        }

        public float[] HtPhasorInPhase(List<IndicatorDataPoint> fxData)
        {
            return HtPhasor(fxData).InPhase;
        }
        public float[] HtPhasorQuadrature(List<IndicatorDataPoint> fxData)
        {
            return HtPhasor(fxData).Quadrature;
        }
        private (float[] InPhase, float[] Quadrature) HtPhasor(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.HtPhasorLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var inPhaseValues = new float[fxData.Count];
            var quadratureValues = new float[fxData.Count];
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
                var inPhaseSignals = new List<float>();
                var quadratureSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    inPhaseSignals.Add(inPhaseValues[i]);
                    quadratureSignals.Add(quadratureValues[i]);
                }

                return (inPhaseSignals.ToArray(), quadratureSignals.ToArray()); // Return the arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Phasor indicator. RetCode: {retCode}");
            }
        }

        public float[] HtSineSine(List<IndicatorDataPoint> fxData)
        {
            return HtSine(fxData).Sine;
        }
        public float[] HtSineLeadSine(List<IndicatorDataPoint> fxData)
        {
            return HtSine(fxData).LeadSine;
        }
        public (float[] Sine, float[] LeadSine) HtSine(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.HtSineLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var sineValues = new float[fxData.Count];
            var leadSineValues = new float[fxData.Count];
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
                var sineSignals = new List<float>();
                var leadSineSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    sineSignals.Add(sineValues[i]);
                    leadSineSignals.Add(leadSineValues[i]);
                }

                return (sineSignals.ToArray(), leadSineSignals.ToArray()); // Return the arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Sine indicator. RetCode: {retCode}");
            }
        }

        public float[] HtTrendline(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.HtTrendlineLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trendlineValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(trendlineValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Trendline indicator. RetCode: {retCode}");
            }
        }
        public float[] HtTrendMode(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.HtTrendModeLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray();

            // Prepare output array
            var trendModeValues = new int[fxData.Count];
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
                return trendModeValues
                    .Skip(outputRange.Start.Value)
                    .Take(outputRange.End.Value - outputRange.Start.Value)
                    .Select(v => (float)v) // Convert int to float for the return type
                    .ToArray();
            }
            else
            {
                throw new Exception($"Error calculating HT Trend Mode indicator. RetCode: {retCode}");
            }
        }

        public float[] Kama(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.KamaLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var kamaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(kamaValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating KAMA indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearReg(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.LinearRegLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(linearRegValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearRegAngle(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.LinearRegAngleLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegAngleValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(linearRegAngleValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Angle indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearRegIntercept(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.LinearRegInterceptLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegInterceptValues = new float[fxData.Count];
            var range = new Range(0, prices.Length-1);

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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(linearRegInterceptValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Intercept indicator. RetCode: {retCode}");
            }
        }
        public float[] LinearRegSlope(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.LinearRegSlopeLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegSlopeValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(linearRegSlopeValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Slope indicator. RetCode: {retCode}");
            }
        }
        public float[] Ln(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.LnLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var lnValues = new float[fxData.Count];
            var range = new Range(0, prices.Length-1);

            // Call Ln function
            var retCode = Functions.Ln<float>(
                prices,
                range,
                lnValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(lnValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Ln indicator. RetCode: {retCode}");
            }
        }
        public float[] Log10(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.Log10Lookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var log10Values = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(log10Values[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Log10 indicator. RetCode: {retCode}");
            }
        }
        public float[] Ma(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30, Core.MAType optInMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MaLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var maValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(maValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MA indicator. RetCode: {retCode}");
            }
        }

        public float[] MacdMACD(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return Macd(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).MACD;
        }
        public float[] MacdSignal(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return Macd(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).Signal;
        }
        public float[] MacdHistogram(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return Macd(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).Histogram;
        }
        private (float[] MACD, float[] Signal, float[] Histogram) Macd(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MacdLookback(optInFastPeriod, optInSlowPeriod, optInSignalPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{optInSignalPeriod}, Slow Period:{optInSlowPeriod} and signal period:{optInSignalPeriod}");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var macdValues = new float[fxData.Count];
            var signalValues = new float[fxData.Count];
            var histogramValues = new float[fxData.Count];
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
                var macdSignals = new List<float>();
                var signalSignals = new List<float>();
                var histogramSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    macdSignals.Add(macdValues[i]);
                    signalSignals.Add(signalValues[i]);
                    histogramSignals.Add(signalValues[i]);
                }

                return (macdSignals.ToArray(), signalSignals.ToArray(), histogramSignals.ToArray());
            }
            else
            {
                throw new Exception($"Error calculating MACD indicator. RetCode: {retCode}");
            }
        }

        public float[] MacdExtMacdSignalss(List<IndicatorDataPoint> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            return MacdExt(fxData, optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod).MacdSignals;
        }
        public float[] MacdExtMacdHistSignals(List<IndicatorDataPoint> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            return MacdExt(fxData, optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod).MacdHistSignals;
        }
        private (float[] MacdSignals, float[] MacdHistSignals) MacdExt(List<IndicatorDataPoint> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MacdExtLookback(optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType,  optInSignalPeriod, optInSignalMAType))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{optInSignalPeriod}, Slow Period:{optInSlowPeriod} and signal period:{optInSignalPeriod}");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
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


        public float[] MacdFixMacdFixSignals(List<IndicatorDataPoint> fxData, int optInSignalPeriod = 9)
        {
            return MacdFix(fxData, optInSignalPeriod).MacdFixSignals;
        }
        public float[] MacdFixMacdFixHistSignals(List<IndicatorDataPoint> fxData, int optInSignalPeriod = 9)
        {
            return MacdFix(fxData, optInSignalPeriod).MacdFixHistSignals;
        }
        private (float[] MacdFixSignals, float[] MacdFixHistSignals) MacdFix(List<IndicatorDataPoint> fxData, int optInSignalPeriod = 9)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MacdFixLookback(optInSignalPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInSignalPeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var macdValues = new float[fxData.Count];
            var macdSignalValues = new float[fxData.Count];
            var macdHistValues = new float[fxData.Count];
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
                var macdFixSignals = new List<float>();
                var macdFixHistSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    macdFixSignals.Add(macdValues[i]);

                    macdFixHistSignals.Add(macdHistValues[i]);
                }

                return (macdFixSignals.ToArray(), macdFixHistSignals.ToArray()); // Return arrays of MACD and Histogram signals
            }
            else
            {
                throw new Exception($"Error calculating MACD (Fix) indicator. RetCode: {retCode}");
            }
        }


        public float[] MamaMamaSignals(List<IndicatorDataPoint> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            return Mama(fxData, optInFastLimit, optInSlowLimit).MamaSignals;
        }
        public float[] MamaFamaSignals(List<IndicatorDataPoint> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            return Mama(fxData, optInFastLimit, optInSlowLimit).FamaSignals;
        }
        private (float[] MamaSignals, float[] FamaSignals) Mama(List<IndicatorDataPoint> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MamaLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period: {optInFastLimit} and Slow Period:{optInSlowLimit}.");
            }


            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var mamaValues = new float[fxData.Count];
            var famaValues = new float[fxData.Count];
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
                var mamaSignals = new List<float>();
                var famaSignals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    mamaSignals.Add(mamaValues[i]);

                    famaSignals.Add(famaValues[i]);
                }

                return (mamaSignals.ToArray(), famaSignals.ToArray()); // Return arrays of MAMA and FAMA signals
            }
            else
            {
                throw new Exception($"Error calculating MAMA indicator. RetCode: {retCode}");
            }
        }

        public float[] Max(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MaxLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var maxValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(maxValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Max indicator. RetCode: {retCode}");
            }
        }
        public float[] MaxIndex(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MaxIndexLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var maxIndices = new int[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Store the index of the maximum value
                    signals.Add(maxIndices[i]);
                }

                return signals.ToArray(); // Return the array of indices
            }
            else
            {
                throw new Exception($"Error calculating MaxIndex indicator. RetCode: {retCode}");
            }
        }
        public float[] MedPrice(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MedPriceLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var medPrices = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(medPrices[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MedPrice indicator. RetCode: {retCode}");
            }
        }

        public float[] MidPoint(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MidPointLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var midPoints = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(midPoints[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MidPoint indicator. RetCode: {retCode}");
            }
        }
        public float[] MidPrice(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MidPriceLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var midPrices = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(midPrices[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MidPrice indicator. RetCode: {retCode}");
            }
        }
        public float[] Min(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.MinLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(minValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Min indicator. RetCode: {retCode}");
            }
        }
        public float[] MinIndex(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MinIndexLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minIndices = new int[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Store the index of the minimum value
                    signals.Add(minIndices[i]);
                }

                return signals.ToArray(); // Return the array of indices
            }
            else
            {
                throw new Exception($"Error calculating MinIndex indicator. RetCode: {retCode}");
            }
        }
        public float[] MinMaxIndex(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MinMaxIndexLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays for min and max indices
            var minIndices = new float[fxData.Count];
            var maxIndices = new float[fxData.Count];
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

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(maxIndices[i] - minIndices[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MinMaxIndex indicator. RetCode: {retCode}");
            }
        }
        public float[] MinusDI(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MinusDILookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minusDIValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(minusDIValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MinusDI indicator. RetCode: {retCode}");
            }
        }
        public float[] MinusDM(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MinusDMLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minusDMValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(minusDMValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MinusDM indicator. RetCode: {retCode}");
            }
        }
        public float[] Mom(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.MomLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var momValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(momValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Mom indicator. RetCode: {retCode}");
            }
        }
        public float[] Mult(List<IndicatorDataPoint> fxData1, List<IndicatorDataPoint> fxData2)
        {
            // Validate input data lengths
            if (fxData1 == null || fxData1.Count < Functions.MultLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            if (fxData1 == null || fxData2 == null || fxData1.Count != fxData2.Count)
            {
                throw new Exception("Both data lists must have the same number of elements to calculate the Mult indicator.");
            }

            // Extract values as floats (using Close prices or any other relevant value)
            var values1 = fxData1.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var values2 = fxData2.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var multValues = new float[fxData1.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(multValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Mult indicator. RetCode: {retCode}");
            }
        }
        public float[] Natr(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.NatrLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var natrValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(natrValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating NATR indicator. RetCode: {retCode}");
            }
        }

        public float[] PlusDI(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.PlusDILookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }


            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var plusDIValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(plusDIValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating PlusDI indicator. RetCode: {retCode}");
            }
        }
        public float[] PlusDM(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.PlusDMLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var plusDMValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(plusDMValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating PlusDM indicator. RetCode: {retCode}");
            }
        }
        public float[] Ppo(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.PpoLookback(optInFastPeriod, optInSlowPeriod, optInMAType))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{optInFastPeriod} and Slow Period: {optInSlowPeriod}");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var ppoValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(ppoValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating PPO indicator. RetCode: {retCode}");
            }
        }
        public float[] Roc(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.RocLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(rocValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROC indicator. RetCode: {retCode}");
            }
        }
        public float[] RocP(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.RocPLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocpValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(rocpValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROCP indicator. RetCode: {retCode}");
            }
        }
        public float[] RocR(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.RocRLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocrValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(rocrValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROCR indicator. RetCode: {retCode}");
            }
        }
        public float[] RocR100(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.RocR100Lookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocr100Values = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(rocr100Values[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROCR100 indicator. RetCode: {retCode}");
            }
        }
        public float[] Rsi(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.RsiLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rsiValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(rsiValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating RSI indicator. RetCode: {retCode}");
            }
        }
        public float[] Sar(List<IndicatorDataPoint> fxData, double optInAcceleration = 0.02, double optInMaximum = 0.2)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SarLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Acceleration:{optInAcceleration} and OptIn Maximum:{optInMaximum}");
            }

            // Extract high and low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sarValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a binary signal: 1 for long, -1 for short (based on SAR positioning)
                    var signal = sarValues[i] > lows[i] ? -1 : 1; // Adjust logic as needed
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the list of signals
            }
            else
            {
                throw new Exception($"Error calculating SAR indicator. RetCode: {retCode}");
            }
        }
        public float[] SarExt(List<IndicatorDataPoint> fxData, double optInStartValue = 0.0, double optInOffsetOnReverse = 0.0, double optInAccelerationInitLong = 0.02, double optInAccelerationLong = 0.02, double optInAccelerationMaxLong = 0.2, double optInAccelerationInitShort = 0.02, double optInAccelerationShort = 0.02, double optInAccelerationMaxShort = 0.2)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SarExtLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high and low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sarValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a binary signal: 1 for long, -1 for short (based on SAR positioning)
                    var signal = sarValues[i] > lows[i] ? -1 : 1; // Adjust logic as needed
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating SAR Ext indicator. RetCode: {retCode}");
            }
        }
        public float[] Sin(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SinLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sinValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(sinValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sin indicator. RetCode: {retCode}");
            }
        }
        public float[] Sinh(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SinhLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sinhValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length - 1);

            // Call Sinh function
            var retCode = Functions.Sinh<float>(
                inputValues,
                range,
                sinhValues,
                out var outputRange);

            // If successful, normalize and return Sinh values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(sinhValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sinh indicator. RetCode: {retCode}");
            }
        }
        public float[] Sma(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SmaLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var smaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(smaValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating SMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Sqrt(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SqrtLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }


            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sqrtValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(sqrtValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sqrt indicator. RetCode: {retCode}");
            }
        }
        public float[] StdDev(List<IndicatorDataPoint> fxData, int optInTimePeriod = 5, double optInNbDev = 1.0)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.StdDevLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";

                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var stdDevValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(stdDevValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating StdDev indicator. RetCode: {retCode}");
            }
        }
        public float[] Stoch(List<IndicatorDataPoint> fxData, int optInFastKPeriod = 5, int optInSlowKPeriod = 3, Core.MAType optInSlowKMAType = Core.MAType.Sma, int optInSlowDPeriod = 3, Core.MAType optInSlowDMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.StochLookback(optInFastKPeriod, optInSlowKPeriod, optInSlowKMAType, optInSlowDPeriod, optInSlowDMAType))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var slowKValues = new float[fxData.Count];
            var slowDValues = new float[fxData.Count];
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

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(slowKValues[i] + slowDValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Stochastic indicator. RetCode: {retCode}");
            }
        }
        public float[] StochF(List<IndicatorDataPoint> fxData, int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma)
        {
            if (fxData == null || fxData.Count < Functions.StochFLookback(optInFastKPeriod, optInFastDPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast K Period: {optInFastKPeriod} and Fast D Period: {optInFastDPeriod}");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var fastKValues = new float[fxData.Count];
            var fastDValues = new float[fxData.Count];
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

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(fastKValues[i] + fastDValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Stochastic Fast indicator. RetCode: {retCode}");
            }
        }
        public float[] StochRsi(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14, int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma)
        {
            if (fxData == null || fxData.Count < Functions.StochRsiLookback(optInFastKPeriod, optInFastDPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast K Period: {optInFastKPeriod} and Fast D Period: {optInFastDPeriod}");
            }

            // Extract close values as floats
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var fastKValues = new float[fxData.Count];
            var fastDValues = new float[fxData.Count];
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

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(fastKValues[i] + fastDValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Stochastic RSI indicator. RetCode: {retCode}");
            }
        }
        public float[] Sub(List<IndicatorDataPoint> fxData0, List<IndicatorDataPoint> fxData1)
        {
            // Validate input data lengths
            if (fxData0 == null || fxData0.Count < Functions.SubLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract input values as floats
            var inputValues0 = fxData0.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var inputValues1 = fxData1.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var subValues = new float[fxData0.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(subValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sub indicator. RetCode: {retCode}");
            }
        }
        public float[] Sum(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.SumLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sumValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(sumValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sum indicator. RetCode: {retCode}");
            }
        }
        public float[] T3(List<IndicatorDataPoint> fxData, int optInTimePeriod = 5, double optInVFactor = 0.7)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.T3Lookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var t3Values = new float[fxData.Count];
            var range = new Range(0, inputValues.Length - 1);

            // Call T3 function
            var retCode = Functions.T3<float>(
                inputValues,
                range,
                t3Values,
                out var outputRange,
                optInTimePeriod,
                optInVFactor);

            // If successful, normalize and return T3 values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(t3Values[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating T3 indicator. RetCode: {retCode}");
            }
        }
        public float[] Tan(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.TanLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }
            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tanValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(tanValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Tan indicator. RetCode: {retCode}");
            }
        }
        public float[] Tanh(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.TanhLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tanhValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length - 1);

            // Call Tanh function
            var retCode = Functions.Tanh<float>(
                inputValues,
                range,
                tanhValues,
                out var outputRange);

            // If successful, normalize and return Tanh values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(tanhValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Tanh indicator. RetCode: {retCode}");
            }
        }
        public float[] Tema(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.TemaLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var temaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(temaValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating TEMA indicator. RetCode: {retCode}");
            }
        }
        public float[] TRange(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.TRangeLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tRangeValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(tRangeValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating True Range indicator. RetCode: {retCode}");
            }
        }
        public float[] Trima(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.TrimaLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trimaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(trimaValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Trima indicator. RetCode: {retCode}");
            }
        }
        public float[] Trix(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {

            int lookBack = Functions.TrixLookback(optInTimePeriod);
            // Validate input data length
            if (fxData == null || fxData.Count < lookBack)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trixValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(trixValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Trix indicator. RetCode: {retCode}");
            }
        }
        public float[] Tsf(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.TsfLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tsfValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(tsfValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating TSF indicator. RetCode: {retCode}");
            }
        }
        public float[] TypPrice(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.TypPriceLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var typPriceValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(typPriceValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Typical Price indicator. RetCode: {retCode}");
            }
        }
        public float[] UltOsc(List<IndicatorDataPoint> fxData, int optInTimePeriod1 = 7, int optInTimePeriod2 = 14, int optInTimePeriod3 = 28)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.UltOscLookback(optInTimePeriod1, optInTimePeriod1, optInTimePeriod3))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Period1:{optInTimePeriod1} and Period2:{optInTimePeriod2} and Period3:{optInTimePeriod3}");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var ultOscValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(ultOscValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Ultimate Oscillator. RetCode: {retCode}");
            }
        }
        public float[] Var(List<IndicatorDataPoint> fxData, int optInTimePeriod = 5)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.VarLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var varValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(varValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Variance indicator. RetCode: {retCode}");
            }
        }
        public float[] WclPrice(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.VarLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(wclPriceValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Weighted Close Price indicator. RetCode: {retCode}");
            }
        }
        public float[] WillR(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Functions.WillRLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var willRValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(willRValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Williams %R indicator. RetCode: {retCode}");
            }
        }
        public float[] Wma(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.WmaLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }
            // Extract the real values (e.g., close prices) as floats
            var realValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var wmaValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(wmaValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating WMA indicator. RetCode: {retCode}");
            }
        }
        public float[] Obv(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.ObvLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract prices and volumes as floats
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var obvValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(obvValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating OBV indicator. RetCode: {retCode}");
            }
        }
        public float[] AdOsc(List<IndicatorDataPoint> fxData, int fastPeriod = 3, int slowPeriod = 10)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.AdOscLookback(fastPeriod, slowPeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period. Fast Period:{fastPeriod} and Slow Period:{slowPeriod}");
            }

            // Extract high, low, close prices, and volume as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume ?? 0m)).ToArray().AsSpan(); // Assuming IndicatorDataPoint has a Volume property

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length || closePrices.Length != volumes.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var adOscValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adOscValue = adOscValues[i];

                    signals.Add(adOscValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating AD Oscillator. RetCode: {retCode}");
            }
        }
        public float[] Mfi(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.MfiLookback(optInTimePeriod))
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period {optInTimePeriod}.");
            }

            // Extract High, Low, Close, and Volume as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var mfiValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    signals.Add(mfiValues[i]);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MFI indicator. RetCode: {retCode}");
            }
        }
        public float[] Ad(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length

            if (fxData == null || fxData.Count < Functions.AdLookback())
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod";
                throw new NotEnoughDataException($"Not enough data to calculate {methodName} indicator for the chosen time period.");
            }

            // Extract high, low, close prices, and volume as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume ?? 0m)).ToArray().AsSpan(); // Assuming IndicatorDataPoint has a Volume property

            // Validate lengths
            if (highPrices.Length != lowPrices.Length || lowPrices.Length != closePrices.Length || closePrices.Length != volumes.Length)
            {
                throw new Exception("Input data lengths do not match.");
            }

            // Prepare output array
            var adValues = new float[fxData.Count];
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
                var signals = new List<float>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adValue = adValues[i];

                    signals.Add(adValue); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating AD indicator. RetCode: {retCode}");
            }
        }

    }
}
