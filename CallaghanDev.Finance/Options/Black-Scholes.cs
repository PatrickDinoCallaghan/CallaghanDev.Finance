
using CallaghanDev.Common.Math;
using CallaghanDev.Utilities;

namespace CallaghanDev.Finance.Options
{
    public class BlackScholes
    {
        private DateTime _maturity;
        private decimal _riskFreeRate;
        private ThreadSafe<decimal> _volatility;

        /// <summary>
        /// Initializes a new instance of the BlackScholes class with the given maturity date and risk-free interest rate.
        /// </summary>
        /// <param name="maturity">The date of option maturity.</param>
        /// <param name="riskFreeRate">The annualized risk-free interest rate.</param>
        public BlackScholes(DateTime maturity, decimal riskFreeRate)
        {
            _volatility = new ThreadSafe<decimal>();
            _maturity = maturity;
            _riskFreeRate = riskFreeRate;
        }

        /// <summary>
        /// Initializes a new instance of the BlackScholes class with the given option data and risk-free interest rate.
        /// </summary>
        /// <param name="optionsData">List of option data.</param>
        /// <param name="riskFreeRate">The annualized risk-free interest rate.</param>
        public BlackScholes(List<OptionData> optionsData, decimal riskFreeRate)
        {
            if (optionsData == null || optionsData.Count == 0)
            {
                throw new ArgumentException("At least one data point is required to calculate volatility.");
            }

            if (!optionsData.All(o => o.Maturity == optionsData.First().Maturity))
            {
                throw new ArgumentException("All options must have the same maturity date.");
            }


            _volatility = new ThreadSafe<decimal>();
            _riskFreeRate = riskFreeRate;
            _maturity = optionsData.First().Maturity;

            decimal volatility = FindVolatility(optionsData);
            _volatility.Value = volatility;
        }

        /// <summary>
        /// Computes the intermediary value d1 in the Black-Scholes formula.
        /// </summary>
        private double CalculateD1(decimal S, decimal K, decimal sigma, decimal t)
        {
            double numerator = Math.Log((double)(S / K)) + (double)((_riskFreeRate + sigma * sigma / 2m) * t);
            double denominator = (double)sigma * Math.Sqrt((double)t);
            return numerator / denominator;
        }

        /// <summary>
        /// Computes the intermediary value d2 in the Black-Scholes formula based on d1.
        /// </summary>
        private double CalculateD2(double d1, decimal sigma, decimal t)
        {
            return d1 - (double)sigma * Math.Sqrt((double)t);
        }

        /// <summary>
        /// Calculates Vega, the derivative of the option price with respect to volatility.
        /// </summary>
        private decimal CalculateVega(decimal S, decimal t, double d1)
        {
            double nd1Prime = 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * d1 * d1);
            return S * (decimal)nd1Prime * (decimal)Math.Sqrt((double)t);
        }

        /// <summary>
        /// Calculates the implied volatility for a single option using the Newton-Raphson method.
        /// </summary>
        private decimal CalculateImpliedVolatility(OptionData option)
        {
            decimal sigma = 0.2m; // Initial guess for volatility
            decimal epsilon = 0.000001m; // Convergence tolerance
            int maxIterations = 100;
            decimal t = (decimal)(option.Maturity - DateTime.Now).TotalDays / 365;

            for (int i = 0; i < maxIterations; i++)
            {
                double d1 = CalculateD1(option.AssetPrice, option.StrikePrice, sigma, t);
                double d2 = CalculateD2(d1, sigma, t);

                decimal calculatedPrice;
                decimal vega;

                if (option.Type == OptionType.Call)
                {
                    calculatedPrice = option.AssetPrice * (decimal)Function.NormalCdf(d1)
                                      - option.StrikePrice * (decimal)Math.Exp((double)(-_riskFreeRate * t)) * (decimal)Function.NormalCdf(d2);
                }
                else
                {
                    calculatedPrice = option.StrikePrice * (decimal)Math.Exp((double)(-_riskFreeRate * t)) * (decimal)Function.NormalCdf(-d2)
                                      - option.AssetPrice * (decimal)Function.NormalCdf(-d1);
                }

                vega = CalculateVega(option.AssetPrice, t, d1);

                decimal priceDifference = calculatedPrice - option.OptionPrice;

                if (Math.Abs(priceDifference) < epsilon)
                {
                    return sigma;
                }

                // Ensure Vega is not zero to avoid division by zero
                if (vega == 0)
                {
                    throw new Exception("Vega is zero. Cannot proceed with Newton-Raphson method.");
                }

                // Update sigma using Newton-Raphson method
                sigma = sigma - priceDifference / vega;

                // Ensure sigma stays positive
                if (sigma <= 0)
                {
                    sigma = epsilon;
                }
            }

            throw new Exception("Implied volatility did not converge.");
        }

        /// <summary>
        /// Finds the average implied volatility from a list of option data.
        /// </summary>
        private decimal FindVolatility(List<OptionData> optionDataList)
        {
            List<decimal> volatilities = new List<decimal>();

            foreach (var option in optionDataList)
            {
                decimal impliedVolatility = CalculateImpliedVolatility(option);
                volatilities.Add(impliedVolatility);
            }

            // Return the average implied volatility
            return volatilities.Average();
        }

        public void UpdateVolatility(List<OptionData> optionDataList)
        {
            _volatility.Value = FindVolatility(optionDataList);
        }
        public void UpdateVolatility(decimal Volatility)
        {
            _volatility.Value = Volatility;
        }

        /// <summary>
        /// Calculates the price of a European call option using the Black-Scholes formula.
        /// </summary>
        public decimal CalculateCallPrice(decimal assetPrice, decimal strikePrice)
        {
            decimal t = (decimal)(_maturity - DateTime.Now).TotalDays / 365;
            double d1 = CalculateD1(assetPrice, strikePrice, _volatility.Value, t);
            double d2 = CalculateD2(d1, _volatility.Value, t);

            return assetPrice * (decimal)Function.NormalCdf(d1)
                   - strikePrice * (decimal)Math.Exp((double)(-_riskFreeRate * t)) * (decimal)Function.NormalCdf(d2);
        }

        /// <summary>
        /// Calculates the price of a European put option using the Black-Scholes formula.
        /// </summary>
        public decimal CalculatePutPrice(decimal assetPrice, decimal strikePrice)
        {
            decimal t = (decimal)(_maturity - DateTime.Now).TotalDays / 365;
            double d1 = CalculateD1(assetPrice, strikePrice, _volatility.Value, t);
            double d2 = CalculateD2(d1, _volatility.Value, t);

            return strikePrice * (decimal)Math.Exp((double)(-_riskFreeRate * t)) * (decimal)Function.NormalCdf(-d2)
                   - assetPrice * (decimal)Function.NormalCdf(-d1);
        }
    }

}