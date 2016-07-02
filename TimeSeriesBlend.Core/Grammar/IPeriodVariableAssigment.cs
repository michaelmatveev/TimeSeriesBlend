using System;
using System.Linq.Expressions;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IPeriodVariableAssigment<I>
    {
        // Shifted vars assign
        IPeriodVariableReader<I> Assign<T>(Expression<Func<T>> basedOn, int shift);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<T>> basedOn, int shift, Func<T> emptyFiller);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<T>> basedOn, int shift, Func<I, T> emptyFiller);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<T>> basedOn, int shift, Func<I, int, T> emptyFiller);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<T>> basedOn, int shift, Func<TimeArg<I>, T> emptyFiller);

        // Calculated value assign
        IPeriodVariableReader<I> Assign<T>(Expression<Func<T>> writer);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<I, T>> writer);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<I, int, T>> writer);
        IPeriodVariableReader<I> Assign<T>(Expression<Func<TimeArg<I>, T>> writer);
    }
}
