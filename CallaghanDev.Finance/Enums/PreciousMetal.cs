using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum PreciousMetal
    {
        [IGTradable("IX.D.SUNGOLD.DAILY.IP", Region.World, Region.CountryVariance)]
        Gold,

        [IGTradable("CS.D.USCSI.TODAY.IP", Region.World, Region.CountryVariance)]
        Silver,

        [IGTradable("MT.D.PA.Month1.IP", Region.World, Region.CountryVariance)]
        Palladium,

        [IGTradable("CS.D.PLAT.TODAY.IP", Region.World, Region.CountryVariance)]
        Platinum
    }
}
