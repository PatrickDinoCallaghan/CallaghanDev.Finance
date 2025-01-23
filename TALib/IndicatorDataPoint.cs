using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TALib
{
    public class IndicatorDataPoint
    {
        public int Id { get; set; }
        public decimal? MidOpen { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public DateTime? DateTime { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Offer { get; set; }

        private decimal? _Close;
        public decimal? Close
        {
            get
            {
                if (_Close.HasValue)
                {
                    return _Close;
                }
                else
                {
                    return (Bid + Offer) / 2;
                }
            }
            set
            {
                _Close = value;
            }
        }
        public decimal? Volume { get; set; }
    }
}
