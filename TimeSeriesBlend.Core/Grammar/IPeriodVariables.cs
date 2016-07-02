using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IPeriodVariables<I>
    {
        IPeriodVariableAssigment<I> Let<T>(Expression<Func<T>> metaVar);
        IPeriodVariableAssigment<I> Let<T>(string name, Expression<Func<T>> metaVar);

        IPeriodVariableAssigment<I> Let<T>(Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn);
        IPeriodVariableAssigment<I> Let<T>(string name, Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn);

        IPeriodVariableAssigment<I> Let<K, T>(Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn);
        IPeriodVariableAssigment<I> Let<K, T>(string name, Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn);

        IEndGroupOrDefinePeriod<I> Summarize(Action<I> action);
        IEndGroupOrDefinePeriod<I> Summarize(Action<I, Int32> action);
        IEndGroupOrDefinePeriod<I> Summarize(Action<TimeArg<I>> action);

        IEndGroupOrDefinePeriod<I> EndPeriod();
    }
}
