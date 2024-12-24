using System.Numerics;

namespace TALib
{
    internal static class CandleHelpers
    {
        public static T RealBody<T>(ReadOnlySpan<T> close, ReadOnlySpan<T> open, int idx) where T : IFloatingPointIeee754<T> =>
            T.Abs(close[idx] - open[idx]);

        public static T UpperShadow<T>(
            ReadOnlySpan<T> high,
            ReadOnlySpan<T> close,
            ReadOnlySpan<T> open,
            int idx) where T : IFloatingPointIeee754<T> => high[idx] - (close[idx] >= open[idx] ? close[idx] : open[idx]);

        public static T LowerShadow<T>(
            ReadOnlySpan<T> close,
            ReadOnlySpan<T> open,
            ReadOnlySpan<T> low,
            int idx) where T : IFloatingPointIeee754<T> => (close[idx] >= open[idx] ? open[idx] : close[idx]) - low[idx];

        public static T HighLowRange<T>(ReadOnlySpan<T> high, ReadOnlySpan<T> low, int idx) where T : IFloatingPointIeee754<T> =>
            high[idx] - low[idx];

        public static Core.CandleColor CandleColor<T>(ReadOnlySpan<T> close, ReadOnlySpan<T> open, int idx)
            where T : IFloatingPointIeee754<T> => close[idx] >= open[idx] ? Core.CandleColor.White : Core.CandleColor.Black;

        public static Core.CandleRangeType CandleRangeType(Core.CandleSettingType set) => Core.CandleSettings.Get(set).RangeType;

        public static int CandleAveragePeriod(Core.CandleSettingType set) => Core.CandleSettings.Get(set).AveragePeriod;

        public static double CandleFactor(Core.CandleSettingType set) => Core.CandleSettings.Get(set).Factor;

        public static T CandleRange<T>(
            ReadOnlySpan<T> open,
            ReadOnlySpan<T> high,
            ReadOnlySpan<T> low,
            ReadOnlySpan<T> close,
            Core.CandleSettingType set,
            int idx) where T : IFloatingPointIeee754<T> => CandleRangeType(set) switch
            {
                Core.CandleRangeType.RealBody => CandleHelpers.RealBody(close, open, idx),
                Core.CandleRangeType.HighLow => HighLowRange(high, low, idx),
                Core.CandleRangeType.Shadows => CandleHelpers.UpperShadow(high, close, open, idx) +
                                            CandleHelpers.LowerShadow(close, open, low, idx),
                _ => T.Zero
            };

        public static T CandleAverage<T>(
            ReadOnlySpan<T> open,
            ReadOnlySpan<T> high,
            ReadOnlySpan<T> low,
            ReadOnlySpan<T> close,
            Core.CandleSettingType set,
            T sum,
            int idx) where T : IFloatingPointIeee754<T>
        {
            var candleAveragePeriod = T.CreateChecked(CandleHelpers.CandleAveragePeriod(set));
            var candleFactor = T.CreateChecked(CandleFactor(set));
            return candleFactor * (!T.IsZero(candleAveragePeriod)
                       ? sum / candleAveragePeriod
                       : CandleHelpers.CandleRange(open, high, low, close, set, idx)) /
                   (CandleRangeType(set) == Core.CandleRangeType.Shadows ? T.CreateChecked(2) : T.One);
        }

        public static bool RealBodyGapUp<T>(
            ReadOnlySpan<T> open,
            ReadOnlySpan<T> close,
            int idx2,
            int idx1) where T : IFloatingPointIeee754<T> => T.Min(open[idx2], close[idx2]) > T.Max(open[idx1], close[idx1]);

        public static bool RealBodyGapDown<T>(
            ReadOnlySpan<T> open,
            ReadOnlySpan<T> close,
            int idx2,
            int idx1) where T : IFloatingPointIeee754<T> => T.Max(open[idx2], close[idx2]) < T.Min(open[idx1], close[idx1]);

        public static bool CandleGapUp<T>(
            ReadOnlySpan<T> low,
            ReadOnlySpan<T> high,
            int idx2,
            int idx1) where T : IFloatingPointIeee754<T> => low[idx2] > high[idx1];

        public static bool CandleGapDown<T>(
            ReadOnlySpan<T> low,
            ReadOnlySpan<T> high,
            int idx2,
            int idx1) where T : IFloatingPointIeee754<T> => high[idx2] < low[idx1];
    }
}

