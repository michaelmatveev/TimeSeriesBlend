using System;
using System.Linq.Expressions;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IPeriodVariableAssigment
    {
        // Shifted vars assign
        IPeriodVariableReader Assign<T>(Expression<Func<T>> basedOn, int shift);
        IPeriodVariableReader Assign<T>(Expression<Func<T>> basedOn, int shift, Func<T> emptyFiller);
        IPeriodVariableReader Assign<T>(Expression<Func<T>> basedOn, int shift, Func<DateTime, T> emptyFiller);
        IPeriodVariableReader Assign<T>(Expression<Func<T>> basedOn, int shift, Func<DateTime, int, T> emptyFiller);
        IPeriodVariableReader Assign<T>(Expression<Func<T>> basedOn, int shift, Func<TimeArg, T> emptyFiller);

        // Calculated value assign
        IPeriodVariableReader Assign<T>(Expression<Func<T>> writer);
        IPeriodVariableReader Assign<T>(Expression<Func<DateTime, T>> writer);
        IPeriodVariableReader Assign<T>(Expression<Func<DateTime, int, T>> writer);
        IPeriodVariableReader Assign<T>(Expression<Func<TimeArg, T>> writer);
    }
}
