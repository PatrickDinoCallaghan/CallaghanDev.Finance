using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.TechnicalAnalysis.Helpers
{
    public static class MovingAverages
    {
        /// <summary>
        /// Computes the Simple Moving Average (SMA) over a set of data points.
        /// SMA = (sum of data points) / (number of data points)
        /// </summary>
        /// <param name="data">The data set to compute the average over.</param>
        /// <returns>The computed SMA as a value of type T.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is empty.</exception>
        public static T Simple<T>(ReadOnlySpan<T> data) where T : IFloatingPoint<T>
        {
            if (data.IsEmpty)
                throw new ArgumentException("Data must not be empty.", nameof(data));

            T sum = T.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

            return sum / T.CreateChecked(data.Length);
        }

        /// <summary>
        /// Computes a simple Weighted Moving Average (WMA), with weights increasing linearly.
        /// For example, the nth-from-last element is multiplied by n, etc.
        /// </summary>
        /// <param name="data">The data set to compute the WMA over.</param>
        /// <returns>The computed WMA as a value of type T.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is empty.</exception>
        public static T Weighted<T>(ReadOnlySpan<T> data) where T : IFloatingPoint<T>
        {
            if (data.IsEmpty)
                throw new ArgumentException("Data must not be empty.", nameof(data));

            T numerator = T.Zero;
            T denominator = T.Zero;

            // Weights: 1, 2, 3, ..., data.Length
            for (int i = 0; i < data.Length; i++)
            {
                var weight = T.CreateChecked(i + 1);
                numerator += data[i] * weight;
                denominator += weight;
            }

            return numerator / denominator;
        }

        /// <summary>
        /// Computes an Exponential Moving Average (EMA) using a user-supplied alpha.
        /// NOTE: This version also applies a 'weight' based on the index, matching your original code.
        /// 
        /// Typical EMA formula: EMA[i] = alpha * data[i] + (1 - alpha) * EMA[i - 1]
        /// In your sample, there's an additional multiplying factor 'weight'. 
        /// Adjust as needed if you want the standard EMA formula instead.
        /// </summary>
        /// <param name="data">The data set to compute the EMA over.</param>
        /// <param name="alpha">Smoothing factor (between 0 and 1).</param>
        /// <returns>The computed EMA as a value of type T.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is empty.</exception>
        public static T Exponential<T>(ReadOnlySpan<T> data, T alpha)
            where T : IFloatingPoint<T>
        {
            if (data.IsEmpty)
                throw new ArgumentException("Data must not be empty.", nameof(data));

            // Start with the first data point
            T ema = data[0];

            // For subsequent points, apply the alpha * data[i] * weight + (1 - alpha) * ema
            for (int i = 1; i < data.Length; i++)
            {
                // weight = i / (data.Length - 1)
                // Must convert i and (data.Length - 1) to T for division
                var weight = T.CreateChecked(i) / T.CreateChecked(data.Length - 1);

                ema = alpha * data[i] * weight + (T.One - alpha) * ema;
            }

            return ema;
        }

        /// <summary>
        /// Overload for ExponentialMovingAverage with a default alpha = 0.3
        /// </summary>
        /// <param name="data">The data set to compute the EMA over.</param>
        /// <returns>The computed EMA with alpha=0.3 as a value of type T.</returns>
        public static T Exponential<T>(ReadOnlySpan<T> data)
            where T : IFloatingPoint<T>
        {
            // Provide a default alpha of 0.3
            T defaultAlpha = T.CreateChecked(0.3);
            return Exponential(data, defaultAlpha);
        }
    }
}
