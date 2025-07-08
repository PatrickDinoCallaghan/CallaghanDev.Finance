namespace CallaghanDev.Finance.Options
{
    public interface IFXOption
    {
        public int OptionDataId { get; set; }

        public int OptionIndex { get; set; }

        public DateTime Timepoint { get; set; }

        public bool? Call { get; set; }

        public bool? Put { get; set; }

        public decimal? AssetOfferHigh { get; set; }

        public decimal? AssetOfferLow { get; set; }

        public decimal? AssetOfferOpen { get; set; }

        public decimal? AssetOfferClose { get; set; }

        public decimal? AssetBidHigh { get; set; }

        public decimal? AssetBidLow { get; set; }

        public decimal? AssetBidOpen { get; set; }

        public decimal? AssetBidClose { get; set; }

        public decimal? OfferHigh { get; set; }

        public decimal? OfferLow { get; set; }

        public decimal? OfferOpen { get; set; }

        public decimal? OfferClose { get; set; }

        public decimal? BidHigh { get; set; }

        public decimal? BidLow { get; set; }

        public decimal? BidOpen { get; set; }

        public decimal? BidClose { get; set; }
        public int? Strike { get; set; }
    }
    public static class FXOptionExtensions
    {
        public static decimal? GetPrice(this IFXOption option)
        {
            if (option.GetBidMid() == null || option.GetOfferMid() == null)
            {
                return null;
            }
            return (option.GetBidMid().Value + option.GetOfferMid().Value) / 2;
        }
        public static decimal? GetBidMid(this IFXOption option)
        {
            if (option.BidHigh.HasValue && option.BidLow.HasValue)
                return (option.BidHigh.Value + option.BidLow.Value) / 2;

            if (option.BidOpen.HasValue && option.BidClose.HasValue)
                return (option.BidOpen.Value + option.BidClose.Value) / 2;

            return null;
        }

        public static decimal? GetOfferMid(this IFXOption option)
        {
            if (option.OfferHigh.HasValue && option.OfferLow.HasValue)
                return (option.OfferHigh.Value + option.OfferLow.Value) / 2;

            if (option.OfferOpen.HasValue && option.OfferClose.HasValue)
                return (option.OfferOpen.Value + option.OfferClose.Value) / 2;

            return null;
        }

        public static decimal? GetAssetPrice(this IFXOption option)
        {
            if (option.GetAssetBidMid() == null || option.GetAssetOfferMid() == null)
            {
                return null;
            }
            return (option.GetAssetBidMid().Value + option.GetAssetOfferMid().Value) / 2;
        }
        public static decimal? GetAssetBidMid(this IFXOption option)
        {
            if (option.AssetBidHigh.HasValue && option.AssetBidLow.HasValue)
                return (option.AssetBidHigh.Value + option.AssetBidLow.Value) / 2;

            if (option.AssetBidOpen.HasValue && option.AssetBidClose.HasValue)
                return (option.AssetBidOpen.Value + option.AssetBidClose.Value) / 2;

            return null;
        }

        public static decimal? GetAssetOfferMid(this IFXOption option)
        {
            if (option.AssetOfferHigh.HasValue && option.AssetOfferLow.HasValue)
                return (option.AssetOfferHigh.Value + option.AssetOfferLow.Value) / 2;

            if (option.AssetOfferOpen.HasValue && option.AssetOfferClose.HasValue)
                return (option.AssetOfferOpen.Value + option.AssetOfferClose.Value) / 2;

            return null;
        }
    }

    public class GarmanKohlhagen
    {
        // Number of minutes per year (365.25 days/year * 24 hours/day * 60 minutes/hour)
        private const double MinsPerYear = 365.25 * 24 * 60;

        /// <summary>
        /// Calculate European call and put option prices using the Garman-Kohlhagen model.
        /// The time to maturity is given in minutes.
        /// </summary>
        /// <param name="spot">Spot exchange rate</param>
        /// <param name="strike">Strike price</param>
        /// <param name="domesticRate">Domestic interest rate (continuously compounded)</param>
        /// <param name="foreignRate">Foreign interest rate (continuously compounded)</param>
        /// <param name="volatility">Annualized volatility</param>
        /// <param name="minutesToMaturity">Time to maturity in minutes</param>
        /// <returns>A tuple with the call and put prices</returns>
        public static (double CallPrice, double PutPrice) CalculateOptionPrices(
            double spot, double strike, double domesticRate, double foreignRate,
            double volatility, double minutesToMaturity)
        {
            // Convert minutes to years
            double T = minutesToMaturity / MinsPerYear;
            double d1 = (Math.Log(spot / strike) + (domesticRate - foreignRate + 0.5 * volatility * volatility) * T) / (volatility * Math.Sqrt(T));
            double d2 = d1 - volatility * Math.Sqrt(T);

            double call = spot * Math.Exp(-foreignRate * T) * NormalCdf(d1) - strike * Math.Exp(-domesticRate * T) * NormalCdf(d2);
            double put = strike * Math.Exp(-domesticRate * T) * NormalCdf(-d2) - spot * Math.Exp(-foreignRate * T) * NormalCdf(-d1);

            return (call, put);
        }

        /// <summary>
        /// Estimate historical annualized volatility from a list of past prices.
        /// The data is assumed to be in minute resolution.
        /// </summary>
        /// <param name="data">List of FX option data</param>
        /// <returns>Annualized volatility</returns>
        public static double EstimateVolatility(List<IFXOption> data)
        {
            // Order the data by Timepoint
            var orderedData = data.OrderBy(d => d.Timepoint).ToList();

            // Compute mid prices from valid data points:
            // Both AssetBidClose and AssetOfferClose must have a valid value (> 0).
            var midPrices = orderedData
                .Select(d =>
                {
                    if (d.AssetBidClose.HasValue && d.AssetOfferClose.HasValue &&
                        d.AssetBidClose.Value > 0 && d.AssetOfferClose.Value > 0)
                    {
                        return (d.AssetBidClose.Value + d.AssetOfferClose.Value) / 2;
                    }
                    else
                    {
                        return (decimal?)null;
                    }
                })
                .Where(price => price.HasValue && price.Value > 0)
                .Select(price => price.Value)
                .ToList();

            if (midPrices.Count < 2)
                throw new InvalidOperationException("Not enough valid price data to estimate volatility.");

            // Calculate log returns from consecutive mid prices
            var logReturns = new List<double>();
            for (int i = 1; i < midPrices.Count; i++)
            {
                decimal previousPrice = midPrices[i - 1];
                decimal currentPrice = midPrices[i];
                double logReturn = Math.Log((double)currentPrice) - Math.Log((double)previousPrice);
                logReturns.Add(logReturn);
            }

            if (logReturns.Count < 2)
                throw new InvalidOperationException("Not enough log return data to estimate volatility.");

            // Compute the mean of the log returns
            double averageReturn = logReturns.Average();

            // Compute the variance of the log returns
            double variance = logReturns.Average(r => Math.Pow(r - averageReturn, 2));
            double stdDev = Math.Sqrt(variance);

            // Annualize volatility using the number of minutes per year.
            return stdDev * Math.Sqrt(MinsPerYear);
        }

        // Cumulative standard normal distribution function
        private static double NormalCdf(double x)
        {
            return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
        }

        // Approximation of the error function (used in NormalCdf)
        private static double Erf(double x)
        {
            // Abramowitz and Stegun formula 7.1.26
            double t = 1.0 / (1.0 + 0.5 * Math.Abs(x));
            double tau = t * Math.Exp(-x * x
                - 1.26551223 + t * (1.00002368 +
                t * (0.37409196 + t * (0.09678418 + t * (-0.18628806 +
                t * (0.27886807 + t * (-1.13520398 + t * (1.48851587 +
                t * (-0.82215223 + t * 0.17087277)))))))));
            return x >= 0 ? 1.0 - tau : tau - 1.0;
        }
    }

}
