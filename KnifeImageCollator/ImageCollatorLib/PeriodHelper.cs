using System;
namespace ImageCollatorLib
{
    public class PeriodHelper
    {
        public static DateTime[] ParsePeriod(string periodStr)
        {
            Period period;
            var ok = Enum.TryParse(periodStr, out period);
            if (ok)
            {
                switch (period)
                {
                    case Period.today:
                        return new DateTime[] { DateTime.Now.Date, DateTime.Now.Date.AddDays(1) };
                    case Period.yesterday:
                        return new DateTime[] { DateTime.Now.Date.AddDays(-1), DateTime.Now.Date };
                    case Period.lastweek:
                        var endLastWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                        var beginLastWeek = endLastWeek.AddDays(-7);
                        return new DateTime[] { beginLastWeek, endLastWeek };
                    case Period.thisweek:
                        var beginThisWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                        var endThisWeek = beginThisWeek.AddDays(7);
                        return new DateTime[] { beginThisWeek, endThisWeek };
                    default:
                        throw new NotImplementedException("Period not implemented: " + period.ToString());
                }
            }
            else
            {
                throw new ArgumentException("Unrecognised: " + periodStr, "period");
            }
        }
    }
}
