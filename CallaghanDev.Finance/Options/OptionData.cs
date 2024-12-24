using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Options
{
    public struct OptionData
    {
        public decimal AssetPrice { get; set; }
        public decimal StrikePrice { get; set; }
        public decimal OptionPrice { get; set; }
        public DateTime Maturity { get; set; }
        public OptionType Type { get; set; }
    }
}
