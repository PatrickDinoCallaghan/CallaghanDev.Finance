using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum EquityIndex
    {
        [IGTradable("IX.D.SPTRD.DAILY.IP", Region.USA)]
        US500,

        [IGTradable("IX.D.SUNDAX.DAILY.IP", Region.Germany)]
        Germany40,

        [IGTradable("IX.D.NIKKEI.DAILY.IP", Region.Japan)]
        Japan225,

        [IGTradable("IX.D.SUNHANS.DAILY.IP", Region.HongKong, Region.China)]
        HongKongHS50,

        [IGTradable("IX.D.CAC.DAILY.IP", Region.France)]
        France40,

        [IGTradable("IX.D.XINHUA.DFB.IP", Region.China)]
        ChinaA50,

        [IGTradable("IX.D.STX600.MONTH2.IP", Region.EU, Region.Germany, Region.France, Region.Spain)]
        EU600,

        [IGTradable("IX.D.IBEX.CASH.IP", Region.Spain)]
        Spain50,

        [IGTradable("IX.D.SMI.DFB.IP", Region.Switzerland)]
        SwitzerlandBlueChip,

        [IGTradable("IX.D.OMX.CASH.IP", Region.Sweden)]
        Sweden30,

        [IGTradable("IX.D.EMGMKT.DFB.IP", Region.Emerging)]
        EmergingMarketsIndex,

        [IGTradable("IX.D.AEX.CASH.IP", Region.Netherlands)]
        Netherlands25,

        [IGTradable("IX.D.SAF.DAILY.IP", Region.SouthAfrica)]
        SouthAfrica40,

        [IGTradable("KB.D.MID250.DAILY.IP", Region.UK)]
        FTSE250,

        [IGTradable("TM.D.BOVESPA.DFB.IP", Region.Brazil)]
        Brazil60,

        [IGTradable("IX.D.SINGAPORE.DFB.IP", Region.Singapore)]
        SingaporeBlueChip,

        [IGTradable("IX.D.DEN25.DAILY.IP", Region.Denmark)]
        Denmark25
    }

}
