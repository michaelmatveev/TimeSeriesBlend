using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IPeriodVariables
    {
        IPeriodVariableAssigment Let<T>(Expression<Func<T>> metaVar);
        IPeriodVariableAssigment Let<T>(string name, Expression<Func<T>> metaVar);

        IPeriodVariableAssigment Let<T>(Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn);
        IPeriodVariableAssigment Let<T>(string name, Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn);

        IPeriodVariableAssigment Let<K, T>(Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn);
        IPeriodVariableAssigment Let<K, T>(string name, Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn);

        IEndGroupOrDefinePeriod Summarize(Action<DateTime> action);
        IEndGroupOrDefinePeriod Summarize(Action<DateTime, Int32> action);
        IEndGroupOrDefinePeriod Summarize(Action<TimeArg> action);

        IEndGroupOrDefinePeriod EndPeriod();
    }
}
