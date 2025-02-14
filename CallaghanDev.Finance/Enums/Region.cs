using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum Region
    {
        Germany,
        Japan,
        France,
        Spain,
        Switzerland,
        Sweden,
        Emerging,
        Netherlands,
        SouthAfrica,
        UK,
        Brazil,
        Singapore,
        Denmark,
        HongKong,
        Italy,
        Australia,
        USA,
        China,
        EU,
        World,
        //This indicates that an asset class has a worldwide impact, but additional data
        //is required to determine the level of dependence for each country, as some are
        //significantly more affected than others.
        CountryVariance
    }
}
