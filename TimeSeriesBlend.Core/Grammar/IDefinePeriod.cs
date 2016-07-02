using System;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IDefinePeriod<I>
    {
        IPeriodVariables<I> BeginPeriod(Func<I, I> getNextPeriod);
        IPeriodVariables<I> BeginPeriod(string caption, Func<I, I> getNextPeriod);
        IPeriodVariables<I> InPeriod(string caption);
        IPeriodVariables<I> InConstants();      
    }

    /// <summary>
    /// EndGroup может быть вызвана только после завершения периода или получения summary для периода
    /// или вызываем любой другой метод из IDefinePeriod
    /// </summary>
    public interface IEndGroupOrDefinePeriod<I> : IDefinePeriod<I>
    {
        IDefineGroup<I> EndGroup();
    }
}
