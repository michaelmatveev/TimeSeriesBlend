using System;

namespace TimeSeriesBlend.Core
{
    public sealed class DateTimeSeriesConnector<H> : SeriesConnector<H, DateTime>
    {
        public DateTimeSeriesConnector(H holder) : base(holder)
        {
        }
    }
}
