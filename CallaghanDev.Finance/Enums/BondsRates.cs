using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public enum BondsRates
    {
        [IGTradable("IR.D.FLG.Month2.IP", Region.UK)]
        UKLongGilt,

        [IGTradable("IR.D.ULTRA100.Month1.IP", Region.USA, Region.World)]
        USUltraTreasuryBond,

        [IGTradable("IR.D.10YEAR100.Month1.IP", Region.USA, Region.World)]
        US10YearTNote,

        [IGTradable("IR.D.BOND100.Month1.IP", Region.USA, Region.World)]
        USTreasuryBond,

        [IGTradable("IR.D.FGBL.Month1.IP", Region.Germany, Region.EU)]
        GermanBund,

        [IGTradable("IR.D.FGBX.Month1.IP", Region.Germany, Region.EU)]
        GermanBuxl,

        [IGTradable("IR.D.JGB.Month3.IP", Region.Japan)]
        JapaneseGovernmentBond,

        [IGTradable("IR.D.FBTP.Month1.IP", Region.Italy, Region.EU)]
        ItalianBTP,

        [IGTradable("IR.D.05YEAR100.Month1.IP", Region.USA, Region.World)]
        US5YearTNote,

        [IGTradable("IR.D.02YEAR100.Month1.IP", Region.USA, Region.World)]
        US2YearTNote,

        [IGTradable("IR.D.FGBM.Month1.IP", Region.Germany, Region.EU)]
        GermanBobl,

        [IGTradable("IR.D.FOAT.Month1.IP", Region.France, Region.EU)]
        FrenchOAT,

        [IGTradable("IR.D.FGBS.Month1.IP", Region.Germany, Region.EU)]
        GermanSchatz,

        [IGTradable("IR.D.FSS.MONTHC.IP", Region.UK)]
        ThreeMonthSONIA,

        [IGTradable("IR.D.SRA.MonthM.IP", Region.USA)]
        ThreeMonthSOFA,

        [IGTradable("IR.D.FEI.Mnth12.IP", Region.EU)]
        Euribor,

        [IGTradable("IR.D.IB.Month8.IP", Region.Australia)]
        Australian30DayInterbankRate,

        [IGTradable("IR.D.FF.MonthX.IP", Region.USA)]
        US30DayFedFundsRate,

        [IGTradable("IX.D.INFLUS.DAILY.IP", Region.USA)]
        USInflationIndex
    }

}
