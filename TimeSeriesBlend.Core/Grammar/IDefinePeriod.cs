using System;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IDefinePeriod
    {
        IPeriodVariables BeginPeriod(Func<DateTime, DateTime> getNextPeriod);
        IPeriodVariables BeginPeriod(string caption, Func<DateTime, DateTime> getNextPeriod);
        IPeriodVariables InPeriod(string caption);
        IPeriodVariables InConstants();      
    }

    /// <summary>
    /// EndGroup может быть вызвана только после завершения периода или получения summary для периода
    /// или вызываем любой другой метод из IDefinePeriod
    /// </summary>
    public interface IEndGroupOrDefinePeriod : IDefinePeriod
    {
        IDefineGroup EndGroup();
    }
}
