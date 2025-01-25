using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.TechnicalAnalysis
{
    /// <summary>
    /// Represents a single data point in a trading or financial context,
    /// with generic support for different numeric types (e.g., decimal, double, float).
    /// </summary>
    /// <typeparam name="T">The numeric type for the data point (must implement IFloatingPoint<T>).</typeparam>
    public interface ITradingDataPoint<T> where T : struct, IFloatingPoint<T> //Need to accept nullable types
    {
        /// <summary>
        /// The mid-price at the opening of the interval.
        /// Typically calculated as (BidOpen + OfferOpen) / 2.
        /// </summary>
        T? Open { get; set; }

        /// <summary>
        /// The highest price observed during the interval.
        /// </summary>
        T? High { get; set; }

        /// <summary>
        /// The lowest price observed during the interval.
        /// </summary>
        T? Low { get; set; }

        /// <summary>
        /// The bid price at the close of the interval (price at which buyers are willing to purchase).
        /// </summary>
        T? Bid { get; set; }

        /// <summary>
        /// The offer (ask) price at the close of the interval (price at which sellers are willing to sell).
        /// </summary>
        T? Offer { get; set; }

        /// <summary>
        /// The closing price of the interval. 
        /// This may be explicitly set or derived as the midpoint of Bid and Offer.
        /// </summary>
        T? Close { get; set; }

        /// <summary>
        /// The total trading volume during the interval.
        /// </summary>
        T? Volume { get; set; }

        /// <summary>
        /// The timestamp or date-time corresponding to the data point.
        /// This represents when the interval ended or when the data point was recorded.
        /// </summary>
        DateTime? DateTime { get; set; }
    }
}
