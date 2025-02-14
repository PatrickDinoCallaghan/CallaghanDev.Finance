using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum SoftCommodity
    {
        [IGTradable("CC.D.CC.USS.IP",Region.World, Region.CountryVariance)]
        Cocoa_NewYork,

        [IGTradable("CC.D.W.USS.IP",Region.World, Region.CountryVariance)]
        Wheat_Chicago,

        [IGTradable("CC.D.KC.USS.IP",Region.World, Region.CountryVariance)]
        ArabicaCoffee_NewYork,

        [IGTradable("CC.D.LCC.USS.IP",Region.World, Region.CountryVariance)]
        Cocoa_London,

        [IGTradable("CC.D.SB.USS.IP",Region.World, Region.CountryVariance)]
        Sugar_NewYork,

        [IGTradable("CC.D.C.USS.IP",Region.World, Region.CountryVariance)]
        Corn,

        [IGTradable("CC.D.BO.USS.IP",Region.World, Region.CountryVariance)]
        SoyBeanOil,

        [IGTradable("CC.D.CT.USS.IP",Region.World, Region.CountryVariance)]
        Cotton,

        [IGTradable("CC.D.LKD.USS.IP",Region.World, Region.CountryVariance)]
        RobustaCoffee_London,

        [IGTradable("CC.D.S.USS.IP",Region.World, Region.CountryVariance)]
        SoyBeans,

        [IGTradable("CC.D.LSU.USS.IP",Region.World, Region.CountryVariance)]
        Sugar_London,

        [IGTradable("CC.D.SM.USS.IP",Region.World, Region.CountryVariance)]
        SoyBeanMeal,

        [IGTradable("CC.D.LC.USS.IP",Region.World, Region.CountryVariance)]
        LiveCattle,

        [IGTradable("CO.D.RR.Month2.IP",Region.World, Region.CountryVariance)]
        RoughRice,

        [IGTradable("CO.D.LH.Month1.IP",Region.World, Region.CountryVariance)]
        LeanHogs,

        [IGTradable("CC.D.LXR.USS.IP",Region.World, Region.CountryVariance)]
        Lumber,

        [IGTradable("CC.D.OJ.USS.IP",Region.World, Region.CountryVariance)]
        OrangeJuice,

        [IGTradable("CO.D.O.Month2.IP",Region.World, Region.CountryVariance)]
        Oats,

        [IGTradable("CO.D.LWB.Month4.IP",Region.World, Region.CountryVariance)]
        Wheat_London
    }
}
