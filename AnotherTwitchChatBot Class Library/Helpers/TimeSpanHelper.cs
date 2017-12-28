using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Helpers
{
    public class TimeSpanHelper
    {
        public static TimeSpan ConvertDurationToTimeSpan(string duration)
        {
            string[] durationSplit = duration.Split(':');
            if (durationSplit.Count() == 2)
                return new TimeSpan(0, int.Parse(durationSplit[0]), int.Parse(durationSplit[1]));
            else
                return new TimeSpan(int.Parse(durationSplit[0]), int.Parse(durationSplit[1]), int.Parse(durationSplit[2]));
        }
    }
}
