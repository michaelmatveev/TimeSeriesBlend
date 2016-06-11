using System;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IPeriodVariableReader
    {
        IPeriodVariableReader Read(Action reader);
        IPeriodVariableReader Read(Action<DateTime> reader);
        IPeriodVariableReader Read(Action<DateTime, int> reader);
        IPeriodVariableReader Read(Action<TimeArg> reader);

        IPeriodVariables End();
    }
}
