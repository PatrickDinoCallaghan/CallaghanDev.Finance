using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum BaseMetal
    {
        [IGTradable("CC.D.HG.USS.IP", Region.World, Region.CountryVariance)]
        HighGradeCopper,

        [IGTradable("CS.D.COPPER.TODAY.IP", Region.World, Region.CountryVariance)]
        Copper,

        [IGTradable("CS.D.ZINC.TODAY.IP", Region.World, Region.CountryVariance)]
        Zinc,

        [IGTradable("CS.D.ALUMINIUM.TODAY.IP",Region.World, Region.CountryVariance)]
        Aluminium,

        [IGTradable("CS.D.LEAD.TODAY.IP",Region.World, Region.CountryVariance)]
        Lead,

        [IGTradable("CC.D.IRON.USS.IP",Region.World, Region.CountryVariance)]
        IronOre,

        [IGTradable("CS.D.NICKEL.TODAY.IP",Region.World, Region.CountryVariance)]
        Nickel
    }
}
