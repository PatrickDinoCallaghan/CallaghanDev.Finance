using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum EnergyType
    {
        [IGTradable("CC.D.CL.USS.IP",Region.World, Region.CountryVariance)]
        Oil_USCrude,

        [IGTradable("CC.D.NG.USS.IP",Region.World, Region.CountryVariance)]
        NaturalGas,

        [IGTradable("CC.D.LCO.USS.IP",Region.World, Region.CountryVariance)]
        Oil_BrentCrude,

        [IGTradable("EN.D.ICENG.Month4.IP",Region.World, Region.CountryVariance)]
        NaturalGasUk,

        [IGTradable("CC.D.HO.USS.IP",Region.World, Region.CountryVariance)]
        HeatingOil,

        [IGTradable("CC.D.RB.USS.IP",Region.World, Region.CountryVariance)]
        Gasoline,

        [IGTradable("CC.D.LGO.USS.IP",Region.World, Region.CountryVariance)]
        London_GasOil,

    }

}
