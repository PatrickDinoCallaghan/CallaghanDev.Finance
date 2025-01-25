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
        /// Computes the Exponential Moving Average (EMA) as a full series.
        /// The first element of the returned array corresponds to the oldest data point (least relevant),
        /// and the last element corresponds to the most current data point (most relevant).
        /// </summary>
        public static T[] Exponential<T>(ReadOnlySpan<T> data, T alpha)
            where T : IFloatingPoint<T>
        {
            if (data.IsEmpty)
                throw new ArgumentException("Data must not be empty.", nameof(data));

            T[] emaValues = new T[data.Length];
            emaValues[0] = data[0]; // Start with the first data point

            // For subsequent points, compute EMA
            for (int i = 1; i < data.Length; i++)
            {
                emaValues[i] = alpha * data[i] + (T.One - alpha) * emaValues[i - 1];
            }

            return emaValues;
        }

        /// <summary>
        /// Overload for ExponentialMovingAverage with a default alpha = 0.3.
        /// </summary>
        public static T[] Exponential<T>(ReadOnlySpan<T> data)
            where T : IFloatingPoint<T>
        {
            T defaultAlpha = T.CreateChecked(0.3);
            return Exponential(data, defaultAlpha);
        }
    }
}
