using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Utility
{
    public static class MathUtil
    {
        public static double GetHarmonicMean(this List<int> tsIndexes)
        {
            double denom = 0;

            foreach (var index in tsIndexes)
                denom += (double)1 / index;

            return (tsIndexes.Count / denom);
        }

        public static TimeSpan GetHarmonicMean(this List<TimeSpan> tsIndexes)
        {
            double denom = 0;

            foreach (var index in tsIndexes)
            {
                denom += 1 / index.TotalSeconds;
            }

            TimeSpan hMean = TimeSpan.FromSeconds(tsIndexes.Count / denom);

            return hMean;
        }
    }
}
