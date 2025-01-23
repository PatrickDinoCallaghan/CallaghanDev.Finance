using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TALib
{

    public class Indicators
    {
        public int[] AccbandsIndicators(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData.Count < 20)
            {
                throw new Exception("Not enough data for the chosen time period (20).");
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

            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var currentPrice = closePrices[i];
                    var upper = upperBand[i];
                    var middle = middleBand[i];
                    var lower = lowerBand[i];

                    if (upper != lower) // Avoid division by zero
                    {
                        // Normalize signal between -100 and +100
                        var signal = ((middle - currentPrice) / (upper - lower)) * 200;

                        if (signal > 100)
                        {
                            signal = 100;
                        }
                        if (signal < -100)
                        {
                            signal = -100;
                        }
                        signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                    }
                    else
                    {
                        signals.Add(0); // Neutral signal in case of invalid bands
                    }
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating acceleration bands. RetCode: {retCode}");
            }

        }
        public int[] AcosIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the ACos indicator.");
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
            var range = new Range(0, closePrices.Length);

            // Call Acos function
            var retCode = Functions.Acos<float>(
                closePrices,
                range,
                acosValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var acosValue = acosValues[i];

                    // Normalize the ACos value between -100 and +100
                    var signal = (acosValue / (float)Math.PI) * 200 - 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ACos indicator. RetCode: {retCode}");
            }
        }

        public int[] AddIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the Add indicator.");
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
            var range = new Range(0, bidPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var addedValue = addedValues[i];

                    // Normalize the added value (scaling between -100 and +100 is hypothetical and depends on the dataset)
                    var signal = (addedValue / (float)(addedValues.Max() - addedValues.Min())) * 200 - 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Add indicator. RetCode: {retCode}");
            }
        }

        public int[] AdxIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod * 2)
            {
                throw new Exception("Not enough data to calculate the ADX indicator.");
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
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adxValue = adxValues[i];

                    // Normalize the ADX value (scaling between 0 and 100 is common for ADX)
                    var signal = (adxValue / 100) * 200 - 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ADX indicator. RetCode: {retCode}");
            }
        }
        public int[] AdxrIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod * 2)
            {
                throw new Exception("Not enough data to calculate the ADXR indicator.");
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
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adxrValue = adxrValues[i];

                    // Normalize the ADXR value (scaling between 0 and 100 is common for ADXR)
                    var signal = (adxrValue / 100) * 200 - 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ADXR indicator. RetCode: {retCode}");
            }
        }
        public int[] ApoIndicator(List<IndicatorDataPoint> fxData, int fastPeriod = 12, int slowPeriod = 26, Core.MAType maType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(fastPeriod, slowPeriod))
            {
                throw new Exception("Not enough data to calculate the APO indicator.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var apoValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var apoValue = apoValues[i];

                    // Normalize the APO value (scaling between -100 and +100 is hypothetical and depends on the dataset)
                    var signal = (apoValue / Math.Max(Math.Abs(apoValues.Max()), Math.Abs(apoValues.Min()))) * 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating APO indicator. RetCode: {retCode}");
            }
        }
        public int[] AroonUpIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            return AroonIndicator(fxData, timePeriod).aroonUp;
        }
        public int[] AroonDownIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            return AroonIndicator(fxData, timePeriod).aroonUp;
        }
        private (int[] aroonUp, int[] aroonDown) AroonIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Aroon indicator.");
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
            var range = new Range(0, highPrices.Length);

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
                var aroonUpSignals = new List<int>();
                var aroonDownSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Aroon Up and Aroon Down values between -100 and +100
                    var aroonUpSignal = (aroonUpValues[i] / 100) * 200 - 100;
                    var aroonDownSignal = (aroonDownValues[i] / 100) * 200 - 100;

                    // Clamp values between -100 and 100
                    aroonUpSignal = Math.Clamp((int)Math.Round(aroonUpSignal), -100, 100);
                    aroonDownSignal = Math.Clamp((int)Math.Round(aroonDownSignal), -100, 100);

                    aroonUpSignals.Add((int)aroonUpSignal);
                    aroonDownSignals.Add((int)aroonDownSignal);
                }

                return (aroonUpSignals.ToArray(), aroonDownSignals.ToArray()); // Return both arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating Aroon indicator. RetCode: {retCode}");
            }
        }
        public int[] AroonOscIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Aroon Oscillator.");
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
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var aroonOscValue = aroonOscValues[i];

                    // Normalize the Aroon Oscillator value between -100 and +100
                    var signal = Math.Clamp((int)Math.Round(aroonOscValue), -100, 100);

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Aroon Oscillator. RetCode: {retCode}");
            }
        }
        public int[] AsinIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the ASIN indicator.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var asinValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length);

            // Call Asin function
            var retCode = Functions.Asin<float>(
                closePrices,
                range,
                asinValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var asinValue = asinValues[i];

                    // Normalize the ASIN value (scaling between -100 and +100 based on asin range [-π/2, π/2])
                    var signal = (asinValue / (float)Math.PI) * 200;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ASIN indicator. RetCode: {retCode}");
            }
        }
        public int[] AtanIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the ATAN indicator.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var atanValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length);

            // Call Atan function
            var retCode = Functions.Atan<float>(
                closePrices,
                range,
                atanValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var atanValue = atanValues[i];

                    // Normalize the ATAN value (scaling between -100 and +100 based on atan range [-π/2, π/2])
                    var signal = (atanValue / (float)Math.PI) * 200;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ATAN indicator. RetCode: {retCode}");
            }
        }
        public int[] AtrIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the ATR indicator.");
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
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var atrValue = atrValues[i];

                    // Normalize the ATR value (scaling is based on the data's range, adjust as needed)
                    var signal = (atrValue / atrValues.Max()) * 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ATR indicator. RetCode: {retCode}");
            }
        }
        public int[] AvgDevIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Average Deviation indicator.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var avgDevValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var avgDevValue = avgDevValues[i];

                    // Normalize the Average Deviation value (scaling between 0 and 100 based on the max value in the dataset)
                    var signal = (avgDevValue / avgDevValues.Max()) * 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Average Deviation indicator. RetCode: {retCode}");
            }
        }
        public int[] AvgPriceIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the Average Price indicator.");
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
            var range = new Range(0, openPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var avgPriceValue = avgPriceValues[i];

                    // Normalize the Average Price value (scaling between 0 and 100 based on the max value in the dataset)
                    var signal = (avgPriceValue / avgPriceValues.Max()) * 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Average Price indicator. RetCode: {retCode}");
            }
        }

        public int[] BbandsUpperBandSignalsIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return BbandsIndicator(fxData, timePeriod, nbDevUp, nbDevDn, maType).upperBandSignals;
        }
        public int[] BbandsMiddleBandSignalsIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return BbandsIndicator(fxData, timePeriod, nbDevUp, nbDevDn, maType).middleBandSignals;
        }
        public int[] BbandsLowerBandSignalsIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            return BbandsIndicator(fxData, timePeriod, nbDevUp, nbDevDn, maType).lowerBandSignals;
        }
        private (int[] upperBandSignals, int[] middleBandSignals, int[] lowerBandSignals) BbandsIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 5, double nbDevUp = 2.0, double nbDevDn = 2.0, Core.MAType maType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Bollinger Bands indicator.");
            }

            // Extract close prices as floats
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var upperBandValues = new float[fxData.Count];
            var middleBandValues = new float[fxData.Count];
            var lowerBandValues = new float[fxData.Count];
            var range = new Range(0, closePrices.Length);

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
                var upperBandSignals = new List<int>();
                var middleBandSignals = new List<int>();
                var lowerBandSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize each band value (scaling between 0 and 100 based on the max values in each band)
                    var upperSignal = (upperBandValues[i] / upperBandValues.Max()) * 100;
                    var middleSignal = (middleBandValues[i] / middleBandValues.Max()) * 100;
                    var lowerSignal = (lowerBandValues[i] / lowerBandValues.Max()) * 100;

                    // Ensure signals stay within bounds
                    upperSignal = Math.Clamp((int)Math.Round(upperSignal), 0, 100);
                    middleSignal = Math.Clamp((int)Math.Round(middleSignal), 0, 100);
                    lowerSignal = Math.Clamp((int)Math.Round(lowerSignal), 0, 100);

                    upperBandSignals.Add((int)upperSignal);
                    middleBandSignals.Add((int)middleSignal);
                    lowerBandSignals.Add((int)lowerSignal);
                }

                return (upperBandSignals.ToArray(), middleBandSignals.ToArray(), lowerBandSignals.ToArray());
            }
            else
            {
                throw new Exception($"Error calculating Bollinger Bands. RetCode: {retCode}");
            }
        }

        public int[] BetaIndicator(List<IndicatorDataPoint> fxData1, List<IndicatorDataPoint> fxData2, int timePeriod = 5)
        {
            // Validate input data length
            if (fxData1 == null || fxData2 == null || fxData1.Count < timePeriod || fxData2.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Beta indicator.");
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
            var range = new Range(0, closePrices1.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var betaValue = betaValues[i];

                    // Normalize the Beta value (assuming range between -1 and +1 for beta)
                    var signal = (betaValue + 1) * 50; // Map -1 to +1 range to 0-100

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < 0)
                    {
                        signal = 0;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Beta indicator. RetCode: {retCode}");
            }
        }
        public int[] BopIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the Balance of Power (BOP) indicator.");
            }

            // Extract open, high, low, and close prices as floats
            var openPrices = fxData.Select(d => Convert.ToSingle(d.MidOpen ?? 0m)).ToArray().AsSpan();
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var bopValues = new float[fxData.Count];
            var range = new Range(0, openPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var bopValue = bopValues[i];

                    // Normalize the BOP value (scaling between -100 and +100 based on the value range of BOP)
                    var signal = Math.Clamp((int)Math.Round(bopValue * 100), -100, 100);

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Balance of Power indicator. RetCode: {retCode}");
            }
        }
        public int[] CciIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Commodity Channel Index (CCI) indicator.");
            }

            // Extract high, low, and close prices as floats
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var cciValues = new float[fxData.Count];
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var cciValue = cciValues[i];

                    // Normalize the CCI value (assuming a range of -200 to 200 for CCI)
                    var signal = Math.Clamp((int)Math.Round((cciValue + 200) / 4), 0, 100); // Map -200 to +200 range to 0-100

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating CCI indicator. RetCode: {retCode}");
            }
        }
        public int[] CeilIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the Ceiling indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var ceilValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Ceil function
            var retCode = Functions.Ceil<float>(
                prices,
                range,
                ceilValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var ceilValue = ceilValues[i];

                    // Normalize the Ceiling value (assuming a range of 0 to 100 based on rounded prices)
                    var signal = Math.Clamp((int)Math.Round(ceilValue), 0, 100);

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Ceiling indicator. RetCode: {retCode}");
            }
        }
        public int[] CmoIndicator(List<IndicatorDataPoint> fxData, int timePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < timePeriod)
            {
                throw new Exception("Not enough data to calculate the Chande Momentum Oscillator (CMO) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var cmoValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var cmoValue = cmoValues[i];

                    // Normalize the CMO value (assuming a range of -100 to 100)
                    var signal = Math.Clamp((int)Math.Round((cmoValue + 100) / 2), 0, 100); // Map -100 to 100 range to 0-100

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating CMO indicator. RetCode: {retCode}");
            }
        }
        public int[] CorrelIndicator(List<IndicatorDataPoint> fxData1, List<IndicatorDataPoint> fxData2, int timePeriod = 30)
        {
            // Validate input data length
            if (fxData1 == null || fxData2 == null || fxData1.Count < timePeriod || fxData2.Count < timePeriod || fxData1.Count != fxData2.Count)
            {
                throw new Exception("Not enough data or mismatched data lengths to calculate the Correlation Coefficient (CORREL) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices1 = fxData1.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var prices2 = fxData2.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var correlValues = new float[fxData1.Count];
            var range = new Range(0, prices1.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var correlValue = correlValues[i];

                    // Normalize the CORREL value (assuming a range of -1 to 1)
                    var signal = Math.Clamp((int)Math.Round((correlValue + 1) * 50), 0, 100); // Map -1 to 1 range to 0-100

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating CORREL indicator. RetCode: {retCode}");
            }
        }
        public int[] CosIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the Cosine (COS) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var cosValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Cos function
            var retCode = Functions.Cos<float>(
                prices,
                range,
                cosValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var cosValue = cosValues[i];

                    // Normalize the COS value (assuming a range of -1 to 1)
                    var signal = Math.Clamp((int)Math.Round((cosValue + 1) * 50), 0, 100); // Map -1 to 1 range to 0-100

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating COS indicator. RetCode: {retCode}");
            }
        }
        public int[] CoshIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the Hyperbolic Cosine (COSH) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var coshValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Cosh function
            var retCode = Functions.Cosh<float>(
                prices,
                range,
                coshValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var coshValue = coshValues[i];

                    // Normalize the COSH value (arbitrary mapping to 0-100, as COSH is not bounded)
                    var signal = Math.Clamp((int)Math.Round(coshValue), 0, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating COSH indicator. RetCode: {retCode}");
            }
        }
        public int[] DemaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Double Exponential Moving Average (DEMA) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var demaValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var demaValue = demaValues[i];

                    // Normalize the DEMA value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(demaValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating DEMA indicator. RetCode: {retCode}");
            }
        }
        public int[] DivIndicator(List<IndicatorDataPoint> fxData, List<IndicatorDataPoint> fxData2)
        {
            // Validate input data lengths
            if (fxData == null || fxData2 == null || fxData.Count != fxData2.Count || fxData.Count == 0)
            {
                throw new Exception("Input data lists must have the same non-zero length.");
            }

            // Extract prices from fxData and fxData2 (using Offer or Bid values)
            var prices1 = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();
            var prices2 = fxData2.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var divValues = new float[fxData.Count];
            var range = new Range(0, prices1.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var divValue = divValues[i];

                    // Normalize the Div value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(divValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Div indicator. RetCode: {retCode}");
            }
        }
        public int[] DxIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the DX indicator.");
            }

            // Extract High, Low, and Close prices
            var highPrices = fxData.Select(d => Convert.ToSingle(d.High)).ToArray().AsSpan();
            var lowPrices = fxData.Select(d => Convert.ToSingle(d.Low)).ToArray().AsSpan();
            var closePrices = fxData.Select(d => Convert.ToSingle(d.Close)).ToArray().AsSpan();

            // Prepare output array
            var dxValues = new float[fxData.Count];
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var dxValue = dxValues[i];

                    // Normalize the DX value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(dxValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating DX indicator. RetCode: {retCode}");
            }
        }
        public int[] EmaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the EMA indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var emaValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var emaValue = emaValues[i];

                    // Normalize the EMA value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(emaValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating EMA indicator. RetCode: {retCode}");
            }
        }
        public int[] ExpIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the Exp indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var expValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Exp function
            var retCode = Functions.Exp<float>(
                prices,
                range,
                expValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var expValue = expValues[i];

                    // Normalize the Exp value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(expValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Exp indicator. RetCode: {retCode}");
            }
        }
        public int[] FloorIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the Floor indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var floorValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Floor function
            var retCode = Functions.Floor<float>(
                prices,
                range,
                floorValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var floorValue = floorValues[i];

                    // Normalize the Floor value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(floorValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Floor indicator. RetCode: {retCode}");
            }
        }
        public int[] HtDcPeriodIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the HT DC Period indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var htDcPeriodValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call HtDcPeriod function
            var retCode = Functions.HtDcPeriod<float>(
                prices,
                range,
                htDcPeriodValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var htDcPeriodValue = htDcPeriodValues[i];

                    // Normalize the HT DC Period value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(htDcPeriodValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating HT DC Period indicator. RetCode: {retCode}");
            }
        }
        public int[] HtDcPhaseIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the HT DC Phase indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var htDcPhaseValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call HtDcPhase function
            var retCode = Functions.HtDcPhase<float>(
                prices,
                range,
                htDcPhaseValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var htDcPhaseValue = htDcPhaseValues[i];

                    // Normalize the HT DC Phase value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(htDcPhaseValue), -100, 100); // Direct mapping for simplicity

                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating HT DC Phase indicator. RetCode: {retCode}");
            }
        }

        public int[] HtPhasorInPhaseIndicator(List<IndicatorDataPoint> fxData)
        {
            return HtPhasorIndicator(fxData).InPhase;
        }
        public int[] HtPhasorQuadratureIndicator(List<IndicatorDataPoint> fxData)
        {
            return HtPhasorIndicator(fxData).Quadrature;
        }
        private (int[] InPhase, int[] Quadrature) HtPhasorIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the HT Phasor indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var inPhaseValues = new float[fxData.Count];
            var quadratureValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var inPhaseSignals = new List<int>();
                var quadratureSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the InPhase and Quadrature values (arbitrary mapping to -100 to 100)
                    var inPhaseSignal = Math.Clamp((int)Math.Round(inPhaseValues[i]), -100, 100);
                    var quadratureSignal = Math.Clamp((int)Math.Round(quadratureValues[i]), -100, 100);

                    inPhaseSignals.Add(inPhaseSignal);
                    quadratureSignals.Add(quadratureSignal);
                }

                return (inPhaseSignals.ToArray(), quadratureSignals.ToArray()); // Return the arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Phasor indicator. RetCode: {retCode}");
            }
        }

        public int[] HtSineSineIndicator(List<IndicatorDataPoint> fxData)
        {
            return HtSineIndicator(fxData).Sine;
        }
        public int[] HtSineLeadSineIndicator(List<IndicatorDataPoint> fxData)
        {
            return HtSineIndicator(fxData).LeadSine;
        }
        public (int[] Sine, int[] LeadSine) HtSineIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the HT Sine indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var sineValues = new float[fxData.Count];
            var leadSineValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var sineSignals = new List<int>();
                var leadSineSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Sine and Lead Sine values (arbitrary mapping to -100 to 100)
                    var sineSignal = Math.Clamp((int)Math.Round(sineValues[i]), -100, 100);
                    var leadSineSignal = Math.Clamp((int)Math.Round(leadSineValues[i]), -100, 100);

                    sineSignals.Add(sineSignal);
                    leadSineSignals.Add(leadSineSignal);
                }

                return (sineSignals.ToArray(), leadSineSignals.ToArray()); // Return the arrays of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Sine indicator. RetCode: {retCode}");
            }
        }

        public int[] HtTrendlineIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the HT Trendline indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trendlineValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call HtTrendline function
            var retCode = Functions.HtTrendline<float>(
                prices,
                range,
                trendlineValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Trendline values (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(trendlineValues[i]), -100, 100);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating HT Trendline indicator. RetCode: {retCode}");
            }
        }
        public int[] HtTrendModeIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the HT Trend Mode indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trendModeValues = new int[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call HtTrendMode function
            var retCode = Functions.HtTrendMode<float>(
                prices,
                range,
                trendModeValues,
                out var outputRange);

            // If successful, return the computed trend mode values
            if (retCode == Core.RetCode.Success)
            {
                return trendModeValues.Skip(outputRange.Start.Value).Take(outputRange.End.Value - outputRange.Start.Value).ToArray();
            }
            else
            {
                throw new Exception($"Error calculating HT Trend Mode indicator. RetCode: {retCode}");
            }
        }
        public int[] KamaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the KAMA indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var kamaValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the KAMA value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(kamaValues[i]), -100, 100);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating KAMA indicator. RetCode: {retCode}");
            }
        }
        public int[] LinearRegIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Linear Regression indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Linear Regression value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(linearRegValues[i]), -100, 100);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression indicator. RetCode: {retCode}");
            }
        }
        public int[] LinearRegAngleIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Linear Regression Angle indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegAngleValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Linear Regression Angle value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(linearRegAngleValues[i]), -100, 100);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Angle indicator. RetCode: {retCode}");
            }
        }
        public int[] LinearRegInterceptIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Linear Regression Intercept indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegInterceptValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Linear Regression Intercept value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(linearRegInterceptValues[i]), -100, 100);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Intercept indicator. RetCode: {retCode}");
            }
        }
        public int[] LinearRegSlopeIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Linear Regression Slope indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var linearRegSlopeValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Linear Regression Slope value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(linearRegSlopeValues[i]), -100, 100);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Linear Regression Slope indicator. RetCode: {retCode}");
            }
        }
        public int[] LnIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the Natural Logarithm (Ln) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var lnValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Ln function
            var retCode = Functions.Ln<float>(
                prices,
                range,
                lnValues,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Ln value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(lnValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Ln indicator. RetCode: {retCode}");
            }
        }
        public int[] Log10Indicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("No data provided to calculate the Log10 indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var log10Values = new float[fxData.Count];
            var range = new Range(0, prices.Length);

            // Call Log10 function
            var retCode = Functions.Log10<float>(
                prices,
                range,
                log10Values,
                out var outputRange);

            // If successful, calculate normalized signals
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Log10 value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(log10Values[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Log10 indicator. RetCode: {retCode}");
            }
        }
        public int[] MaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30, Core.MAType optInMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Moving Average (MA) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var maValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MA value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(maValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MA indicator. RetCode: {retCode}");
            }
        }

        public int[] MacdMACDIndicator(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return MacdIndicator(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).MACD;
        }
        public int[] MacdSignalIndicator(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return MacdIndicator(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).Signal;
        }
        public int[] MacdHistogramIndicator(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            return MacdIndicator(fxData, optInFastPeriod, optInSlowPeriod, optInSignalPeriod).Histogram;
        }
        private (int[] MACD, int[] Signal, int[] Histogram) MacdIndicator(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(optInFastPeriod, Math.Max(optInSlowPeriod, optInSignalPeriod)))
            {
                throw new Exception("Not enough data to calculate the MACD indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var macdValues = new float[fxData.Count];
            var signalValues = new float[fxData.Count];
            var histogramValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var macdSignals = new List<int>();
                var signalSignals = new List<int>();
                var histogramSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MACD, Signal, and Histogram values (arbitrary mapping to -100 to 100)
                    macdSignals.Add(Math.Clamp((int)Math.Round(macdValues[i] * 10), -100, 100));
                    signalSignals.Add(Math.Clamp((int)Math.Round(signalValues[i] * 10), -100, 100));
                    histogramSignals.Add(Math.Clamp((int)Math.Round(histogramValues[i] * 10), -100, 100));
                }

                return (macdSignals.ToArray(), signalSignals.ToArray(), histogramSignals.ToArray());
            }
            else
            {
                throw new Exception($"Error calculating MACD indicator. RetCode: {retCode}");
            }
        }

        public int[] MacdExtMacdSignalssIndicator(List<IndicatorDataPoint> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            return MacdExtIndicator(fxData, optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod).MacdSignals;
        }
        public int[] MacdExtMacdHistSignalsIndicator(List<IndicatorDataPoint> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            return MacdExtIndicator(fxData, optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod).MacdHistSignals;
        }
        private (int[] MacdSignals, int[] MacdHistSignals) MacdExtIndicator(List<IndicatorDataPoint> fxData,
            int optInFastPeriod = 12, Core.MAType optInFastMAType = Core.MAType.Sma,
            int optInSlowPeriod = 26, Core.MAType optInSlowMAType = Core.MAType.Sma,
            int optInSignalPeriod = 9, Core.MAType optInSignalMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(optInFastPeriod, Math.Max(optInSlowPeriod, optInSignalPeriod)))
            {
                throw new Exception("Not enough data to calculate the MACD (Ext) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var macdValues = new float[fxData.Count];
            var macdSignalValues = new float[fxData.Count];
            var macdHistValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var macdSignals = new List<int>();
                var macdHistSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MACD value (arbitrary mapping to -100 to 100)
                    var macdSignal = Math.Clamp((int)Math.Round(macdValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    macdSignals.Add(macdSignal);

                    // Normalize the MACD Histogram value (arbitrary mapping to -100 to 100)
                    var macdHistSignal = Math.Clamp((int)Math.Round(macdHistValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    macdHistSignals.Add(macdHistSignal);
                }

                return (macdSignals.ToArray(), macdHistSignals.ToArray()); // Return arrays of MACD and Histogram signals
            }
            else
            {
                throw new Exception($"Error calculating MACD (Ext) indicator. RetCode: {retCode}");
            }
        }


        public int[] MacdFixMacdFixSignalsIndicator(List<IndicatorDataPoint> fxData, int optInSignalPeriod = 9)
        {
            return MacdFixIndicator(fxData, optInSignalPeriod).MacdFixSignals;
        }
        public int[] MacdFixMacdFixHistSignalsIndicator(List<IndicatorDataPoint> fxData, int optInSignalPeriod = 9)
        {
            return MacdFixIndicator(fxData, optInSignalPeriod).MacdFixHistSignals;
        }
        private (int[] MacdFixSignals, int[] MacdFixHistSignals) MacdFixIndicator(List<IndicatorDataPoint> fxData, int optInSignalPeriod = 9)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 26 + optInSignalPeriod)
            {
                throw new Exception("Not enough data to calculate the MACD (Fix) indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var macdValues = new float[fxData.Count];
            var macdSignalValues = new float[fxData.Count];
            var macdHistValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var macdFixSignals = new List<int>();
                var macdFixHistSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MACD value (arbitrary mapping to -100 to 100)
                    var macdFixSignal = Math.Clamp((int)Math.Round(macdValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    macdFixSignals.Add(macdFixSignal);

                    // Normalize the MACD Histogram value (arbitrary mapping to -100 to 100)
                    var macdFixHistSignal = Math.Clamp((int)Math.Round(macdHistValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    macdFixHistSignals.Add(macdFixHistSignal);
                }

                return (macdFixSignals.ToArray(), macdFixHistSignals.ToArray()); // Return arrays of MACD and Histogram signals
            }
            else
            {
                throw new Exception($"Error calculating MACD (Fix) indicator. RetCode: {retCode}");
            }
        }


        public int[] MamaMamaSignalsIndicator(List<IndicatorDataPoint> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            return MamaIndicator(fxData, optInFastLimit, optInSlowLimit).MamaSignals;
        }
        public int[] MamaFamaSignalsIndicator(List<IndicatorDataPoint> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            return MamaIndicator(fxData, optInFastLimit, optInSlowLimit).FamaSignals;
        }
        private (int[] MamaSignals, int[] FamaSignals) MamaIndicator(List<IndicatorDataPoint> fxData, double optInFastLimit = 0.5, double optInSlowLimit = 0.05)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 32) // MamaLookback specifies a fixed lookback of 32
            {
                throw new Exception("Not enough data to calculate the MAMA indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var mamaValues = new float[fxData.Count];
            var famaValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var mamaSignals = new List<int>();
                var famaSignals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MAMA value (arbitrary mapping to -100 to 100)
                    var mamaSignal = Math.Clamp((int)Math.Round(mamaValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    mamaSignals.Add(mamaSignal);

                    // Normalize the FAMA value (arbitrary mapping to -100 to 100)
                    var famaSignal = Math.Clamp((int)Math.Round(famaValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    famaSignals.Add(famaSignal);
                }

                return (mamaSignals.ToArray(), famaSignals.ToArray()); // Return arrays of MAMA and FAMA signals
            }
            else
            {
                throw new Exception($"Error calculating MAMA indicator. RetCode: {retCode}");
            }
        }

        public int[] MaxIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Max indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var maxValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Max value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(maxValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Max indicator. RetCode: {retCode}");
            }
        }
        public int[] MaxIndexIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MaxIndex indicator.");
            }

            // Extract prices as floats (using Offer or Bid values)
            var prices = fxData.Select(d => Convert.ToSingle(d.Offer ?? d.Bid ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var maxIndices = new int[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

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
        public int[] MedPriceIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the MedPrice indicator.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var medPrices = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MedPrice value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(medPrices[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MedPrice indicator. RetCode: {retCode}");
            }
        }

        public int[] MidPointIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MidPoint indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var midPoints = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MidPoint value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(midPoints[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MidPoint indicator. RetCode: {retCode}");
            }
        }
        public int[] MidPriceIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MidPrice indicator.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var midPrices = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MidPrice value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(midPrices[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MidPrice indicator. RetCode: {retCode}");
            }
        }
        public int[] MinIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Min indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Min value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(minValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Min indicator. RetCode: {retCode}");
            }
        }
        public int[] MinIndexIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MinIndex indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minIndices = new int[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

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
        public int[] MinMaxIndexIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MinMaxIndex indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays for min and max indices
            var minIndices = new float[fxData.Count];
            var maxIndices = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Min and Max indices (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round((maxIndices[i] - minIndices[i]) * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MinMaxIndex indicator. RetCode: {retCode}");
            }
        }
        public int[] MinusDIIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MinusDI indicator.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minusDIValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MinusDI value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(minusDIValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MinusDI indicator. RetCode: {retCode}");
            }
        }
        public int[] MinusDMIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MinusDM indicator.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var minusDMValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MinusDM value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(minusDMValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MinusDM indicator. RetCode: {retCode}");
            }
        }
        public int[] MomIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Mom indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var momValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Mom value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(momValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Mom indicator. RetCode: {retCode}");
            }
        }
        public int[] MultIndicator(List<IndicatorDataPoint> fxData1, List<IndicatorDataPoint> fxData2)
        {
            // Validate input data lengths
            if (fxData1 == null || fxData2 == null || fxData1.Count != fxData2.Count)
            {
                throw new Exception("Both data lists must have the same number of elements to calculate the Mult indicator.");
            }

            // Extract values as floats (using Close prices or any other relevant value)
            var values1 = fxData1.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var values2 = fxData2.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var multValues = new float[fxData1.Count];
            var range = new Range(0, values1.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the Mult value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(multValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Mult indicator. RetCode: {retCode}");
            }
        }
        public int[] NatrIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the NATR indicator.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var natrValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the NATR value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(natrValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating NATR indicator. RetCode: {retCode}");
            }
        }

        public int[] PlusDIIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the PlusDI indicator.");
            }

            // Extract High, Low, and Close prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var plusDIValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the PlusDI value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(plusDIValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating PlusDI indicator. RetCode: {retCode}");
            }
        }
        public int[] PlusDMIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the PlusDM indicator.");
            }

            // Extract High and Low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var plusDMValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the PlusDM value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(plusDMValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating PlusDM indicator. RetCode: {retCode}");
            }
        }
        public int[] PpoIndicator(List<IndicatorDataPoint> fxData, int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(optInFastPeriod, optInSlowPeriod))
            {
                throw new Exception("Not enough data to calculate the PPO indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var ppoValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the PPO value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(ppoValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating PPO indicator. RetCode: {retCode}");
            }
        }
        public int[] RocIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the ROC indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the ROC value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(rocValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROC indicator. RetCode: {retCode}");
            }
        }
        public int[] RocPIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the ROCP indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocpValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the ROCP value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(rocpValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROCP indicator. RetCode: {retCode}");
            }
        }
        public int[] RocRIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the ROCR indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocrValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the ROCR value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(rocrValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROCR indicator. RetCode: {retCode}");
            }
        }
        public int[] RocR100Indicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the ROCR100 indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rocr100Values = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the ROCR100 value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(rocr100Values[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating ROCR100 indicator. RetCode: {retCode}");
            }
        }
        public int[] RsiIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the RSI indicator.");
            }

            // Extract prices as floats (using Close prices or any other relevant value)
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var rsiValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the RSI value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(rsiValues[i] - 50) * 2, -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating RSI indicator. RetCode: {retCode}");
            }
        }
        public int[] SarIndicator(List<IndicatorDataPoint> fxData, double optInAcceleration = 0.02, double optInMaximum = 0.2)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 2)
            {
                throw new Exception("Not enough data to calculate the SAR indicator.");
            }

            // Extract high and low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sarValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

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
        public int[] SarExtIndicator(List<IndicatorDataPoint> fxData, double optInStartValue = 0.0, double optInOffsetOnReverse = 0.0, double optInAccelerationInitLong = 0.02, double optInAccelerationLong = 0.02, double optInAccelerationMaxLong = 0.2, double optInAccelerationInitShort = 0.02, double optInAccelerationShort = 0.02, double optInAccelerationMaxShort = 0.2)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 2)
            {
                throw new Exception("Not enough data to calculate the SAR Ext indicator.");
            }

            // Extract high and low prices as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sarValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

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
        public int[] SinIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Sin indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sinValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

            // Call Sin function
            var retCode = Functions.Sin<float>(
                inputValues,
                range,
                sinValues,
                out var outputRange);

            // If successful, normalize and return Sin values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Sin value (arbitrary scaling)
                    var signal = (int)Math.Round(sinValues[i] * 100); // Scale sin values to range [-100, 100]
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sin indicator. RetCode: {retCode}");
            }
        }
        public int[] SinhIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Sinh indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sinhValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

            // Call Sinh function
            var retCode = Functions.Sinh<float>(
                inputValues,
                range,
                sinhValues,
                out var outputRange);

            // If successful, normalize and return Sinh values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Sinh value (arbitrary scaling)
                    var signal = (int)Math.Round(sinhValues[i] * 100); // Scale sinh values to range [-100, 100]
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sinh indicator. RetCode: {retCode}");
            }
        }
        public int[] SmaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the SMA indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var smaValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the SMA value (arbitrary scaling)
                    var signal = (int)Math.Round(smaValues[i]); // Use rounded SMA values as signals
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating SMA indicator. RetCode: {retCode}");
            }
        }
        public int[] SqrtIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Sqrt indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sqrtValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

            // Call Sqrt function
            var retCode = Functions.Sqrt<float>(
                inputValues,
                range,
                sqrtValues,
                out var outputRange);

            // If successful, normalize and return Sqrt values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Sqrt value (arbitrary scaling)
                    var signal = (int)Math.Round(sqrtValues[i] * 100); // Scale sqrt values for easier interpretation
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sqrt indicator. RetCode: {retCode}");
            }
        }
        public int[] StdDevIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 5, double optInNbDev = 1.0)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the StdDev indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var stdDevValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the StdDev value (arbitrary scaling)
                    var signal = (int)Math.Round(stdDevValues[i] * 100); // Scale std dev values for easier interpretation
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating StdDev indicator. RetCode: {retCode}");
            }
        }
        public int[] StochIndicator(List<IndicatorDataPoint> fxData, int optInFastKPeriod = 5, int optInSlowKPeriod = 3, Core.MAType optInSlowKMAType = Core.MAType.Sma, int optInSlowDPeriod = 3, Core.MAType optInSlowDMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(optInFastKPeriod, Math.Max(optInSlowKPeriod, optInSlowDPeriod)))
            {
                throw new Exception("Not enough data to calculate the Stochastic indicator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var slowKValues = new float[fxData.Count];
            var slowDValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Combine SlowK and SlowD values into a signal (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round((slowKValues[i] + slowDValues[i]) / 2 * 100); // Scale and average
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Stochastic indicator. RetCode: {retCode}");
            }
        }
        public int[] StochFIndicator(List<IndicatorDataPoint> fxData, int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(optInFastKPeriod, optInFastDPeriod))
            {
                throw new Exception("Not enough data to calculate the Stochastic Fast indicator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var fastKValues = new float[fxData.Count];
            var fastDValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Combine FastK and FastD values into a signal (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round((fastKValues[i] + fastDValues[i]) / 2 * 100); // Scale and average
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Stochastic Fast indicator. RetCode: {retCode}");
            }
        }
        public int[] StochRsiIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14, int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(optInTimePeriod, Math.Max(optInFastKPeriod, optInFastDPeriod)))
            {
                throw new Exception("Not enough data to calculate the Stochastic RSI indicator.");
            }

            // Extract close values as floats
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output arrays
            var fastKValues = new float[fxData.Count];
            var fastDValues = new float[fxData.Count];
            var range = new Range(0, closes.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Combine FastK and FastD values into a signal (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round((fastKValues[i] + fastDValues[i]) / 2 * 100); // Scale and average
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Stochastic RSI indicator. RetCode: {retCode}");
            }
        }
        public int[] SubIndicator(List<IndicatorDataPoint> fxData0, List<IndicatorDataPoint> fxData1)
        {
            // Validate input data lengths
            if (fxData0 == null || fxData1 == null || fxData0.Count != fxData1.Count || fxData0.Count == 0)
            {
                throw new Exception("Invalid or mismatched data for the Sub indicator.");
            }

            // Extract input values as floats
            var inputValues0 = fxData0.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var inputValues1 = fxData1.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var subValues = new float[fxData0.Count];
            var range = new Range(0, inputValues0.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Sub value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(subValues[i] * 100); // Scale the difference values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sub indicator. RetCode: {retCode}");
            }
        }
        public int[] SumIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Sum indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var sumValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Sum value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(sumValues[i]); // Use the summed values as signals
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Sum indicator. RetCode: {retCode}");
            }
        }
        public int[] T3Indicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 5, double optInVFactor = 0.7)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the T3 indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var t3Values = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the T3 value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(t3Values[i] * 100); // Scale the T3 values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating T3 indicator. RetCode: {retCode}");
            }
        }
        public int[] TanIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Tan indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tanValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

            // Call Tan function
            var retCode = Functions.Tan<float>(
                inputValues,
                range,
                tanValues,
                out var outputRange);

            // If successful, normalize and return Tan values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Tan value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(tanValues[i] * 100); // Scale the Tan values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Tan indicator. RetCode: {retCode}");
            }
        }
        public int[] TanhIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Tanh indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tanhValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

            // Call Tanh function
            var retCode = Functions.Tanh<float>(
                inputValues,
                range,
                tanhValues,
                out var outputRange);

            // If successful, normalize and return Tanh values
            if (retCode == Core.RetCode.Success)
            {
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Tanh value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(tanhValues[i] * 100); // Scale the Tanh values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Tanh indicator. RetCode: {retCode}");
            }
        }
        public int[] TemaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the TEMA indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var temaValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the TEMA value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(temaValues[i] * 100); // Scale the TEMA values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating TEMA indicator. RetCode: {retCode}");
            }
        }
        public int[] TRangeIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 2)
            {
                throw new Exception("Not enough data to calculate the True Range indicator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tRangeValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the True Range value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(tRangeValues[i] * 100); // Scale the True Range values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating True Range indicator. RetCode: {retCode}");
            }
        }
        public int[] TrimaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Trima indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trimaValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Trima value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(trimaValues[i] * 100); // Scale the Trima values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Trima indicator. RetCode: {retCode}");
            }
        }
        public int[] TrixIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Trix indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var trixValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Trix value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(trixValues[i] * 100); // Scale the Trix values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Trix indicator. RetCode: {retCode}");
            }
        }
        public int[] TsfIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the TSF indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var tsfValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the TSF value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(tsfValues[i] * 100); // Scale the TSF values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating TSF indicator. RetCode: {retCode}");
            }
        }
        public int[] TypPriceIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Typical Price indicator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var typPriceValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Typical Price value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(typPriceValues[i] * 100); // Scale the Typical Price values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Typical Price indicator. RetCode: {retCode}");
            }
        }
        public int[] UltOscIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod1 = 7, int optInTimePeriod2 = 14, int optInTimePeriod3 = 28)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(Math.Max(optInTimePeriod1, optInTimePeriod2), optInTimePeriod3))
            {
                throw new Exception("Not enough data to calculate the Ultimate Oscillator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var ultOscValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Ultimate Oscillator value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(ultOscValues[i] * 100); // Scale the Ultimate Oscillator values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Ultimate Oscillator. RetCode: {retCode}");
            }
        }
        public int[] VarIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 5)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Variance indicator.");
            }

            // Extract input values as floats
            var inputValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var varValues = new float[fxData.Count];
            var range = new Range(0, inputValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Variance value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(varValues[i] * 100); // Scale the Variance values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Variance indicator. RetCode: {retCode}");
            }
        }
        public int[] WclPriceIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the Weighted Close Price indicator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var wclPriceValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Weighted Close Price value (arbitrary scaling or mapping logic can be applied here)
                    var signal = (int)Math.Round(wclPriceValues[i] * 100); // Scale the Weighted Close Price values
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Weighted Close Price indicator. RetCode: {retCode}");
            }
        }
        public int[] WillRIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the Williams %R indicator.");
            }

            // Extract high, low, and close values as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var willRValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the Williams %R value (scaled from -100 to 0)
                    var signal = (int)Math.Round(willRValues[i]);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating Williams %R indicator. RetCode: {retCode}");
            }
        }
        public int[] WmaIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 30)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the WMA indicator.");
            }

            // Extract the real values (e.g., close prices) as floats
            var realValues = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var wmaValues = new float[fxData.Count];
            var range = new Range(0, realValues.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Generate a signal based on the WMA value (rounded to the nearest integer)
                    var signal = (int)Math.Round(wmaValues[i]);
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating WMA indicator. RetCode: {retCode}");
            }
        }
        public int[] ObvIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count == 0)
            {
                throw new Exception("Not enough data to calculate the OBV indicator.");
            }

            // Extract prices and volumes as floats
            var prices = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var obvValues = new float[fxData.Count];
            var range = new Range(0, prices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the OBV value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(obvValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating OBV indicator. RetCode: {retCode}");
            }
        }
        public int[] AdOscIndicator(List<IndicatorDataPoint> fxData, int fastPeriod = 3, int slowPeriod = 10)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < Math.Max(fastPeriod, slowPeriod))
            {
                throw new Exception("Not enough data to calculate the AD Oscillator.");
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
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adOscValue = adOscValues[i];

                    // Normalize the AD Oscillator value (scaling between -100 and +100 is hypothetical and depends on the dataset)
                    var signal = (adOscValue / (float)(adOscValues.Max() - adOscValues.Min())) * 200 - 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating AD Oscillator. RetCode: {retCode}");
            }
        }
        public int[] MfiIndicator(List<IndicatorDataPoint> fxData, int optInTimePeriod = 14)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < optInTimePeriod)
            {
                throw new Exception("Not enough data to calculate the MFI indicator.");
            }

            // Extract High, Low, Close, and Volume as floats
            var highs = fxData.Select(d => Convert.ToSingle(d.High ?? 0m)).ToArray().AsSpan();
            var lows = fxData.Select(d => Convert.ToSingle(d.Low ?? 0m)).ToArray().AsSpan();
            var closes = fxData.Select(d => Convert.ToSingle(d.Close ?? 0m)).ToArray().AsSpan();
            var volumes = fxData.Select(d => Convert.ToSingle(d.Volume ?? 0m)).ToArray().AsSpan();

            // Prepare output array
            var mfiValues = new float[fxData.Count];
            var range = new Range(0, highs.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    // Normalize the MFI value (arbitrary mapping to -100 to 100)
                    var signal = Math.Clamp((int)Math.Round(mfiValues[i] * 10), -100, 100); // Adjust scaling if necessary
                    signals.Add(signal);
                }

                return signals.ToArray(); // Return the array of signals
            }
            else
            {
                throw new Exception($"Error calculating MFI indicator. RetCode: {retCode}");
            }
        }
        public int[] AdIndicator(List<IndicatorDataPoint> fxData)
        {
            // Validate input data length
            if (fxData == null || fxData.Count < 1)
            {
                throw new Exception("Not enough data to calculate the AD indicator.");
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
            var range = new Range(0, highPrices.Length);

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
                var signals = new List<int>();

                for (int i = outputRange.Start.Value; i < outputRange.End.Value; i++)
                {
                    var adValue = adValues[i];

                    // Normalize the AD value (scaling between -100 and +100 is hypothetical here and depends on the dataset)
                    var signal = adValue / (float)(adValues.Max() - adValues.Min()) * 200 - 100;

                    if (signal > 100)
                    {
                        signal = 100;
                    }
                    if (signal < -100)
                    {
                        signal = -100;
                    }

                    signals.Add((int)Math.Round(signal)); // Round to nearest int and add to signals
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
