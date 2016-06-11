using System;

namespace TimeSeriesBlend.Core
{
    public static class StandardPeriods
    {
        public static Func<DateTime, DateTime> Year     = (date) => date.AddYears(1);
        public static Func<DateTime, DateTime> Month    = (date) => date.AddMonths(1);
        public static Func<DateTime, DateTime> Day      = (date) => date.AddDays(1);
        public static Func<DateTime, DateTime> Hour     = (date) => date.AddHours(1);
        public static Func<DateTime, DateTime> Minute   = (date) => date.AddMinutes(1);
    }
}
