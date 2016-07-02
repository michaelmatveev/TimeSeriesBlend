using System;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IPeriodVariableReader<I>
    {
        IPeriodVariableReader<I> Read(Action reader);
        IPeriodVariableReader<I> Read(Action<I> reader);
        IPeriodVariableReader<I> Read(Action<I, int> reader);
        IPeriodVariableReader<I> Read(Action<TimeArg<I>> reader);

        IPeriodVariables<I> End();
    }
}
