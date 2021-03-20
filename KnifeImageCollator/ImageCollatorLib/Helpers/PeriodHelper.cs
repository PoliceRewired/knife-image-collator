using System;
using System.Globalization;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Helpers
{
    public class PeriodHelper
    {
        public static DateTime[] ParsePeriod(string periodStr)
        {
            Periods period;
            var ok = Enum.TryParse(periodStr, out period);
            if (ok)
            {
                switch (period)
                {
                    case Periods.today:
                        return new DateTime[] { DateTime.Now.Date, DateTime.Now.Date.AddDays(1) };
                    case Periods.yesterday:
                        return new DateTime[] { DateTime.Now.Date.AddDays(-1), DateTime.Now.Date };
                    case Periods.lastweek:
                        var endLastWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                        var beginLastWeek = endLastWeek.AddDays(-7);
                        return new DateTime[] { beginLastWeek, endLastWeek };
                    case Periods.thisweek:
                        var beginThisWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                        var endThisWeek = beginThisWeek.AddDays(7);
                        return new DateTime[] { beginThisWeek, endThisWeek };
                    default:
                        throw new NotImplementedException("Period not implemented: " + period.ToString());
                }
            }
            else
            {
                if (periodStr.Contains(":"))
                {
                    var parts = periodStr.Split(':');
                    var start = DateTime.ParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var end = DateTime.ParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return new DateTime[] { start, end };
                }

                throw new ArgumentException("Unrecognised: " + periodStr, "period");
            }
        }
    }
}
