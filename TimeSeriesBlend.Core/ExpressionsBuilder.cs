using System;
using System.Linq.Expressions;

namespace TimeSeriesBlend.Core
{
    internal static class ExpressionsBuilder
    {
        public static Expression<Func<TimeArg, T>> ConvertExpression<T>(Expression<Func<T>> input)
        {
            ParameterExpression tp = Expression.Parameter(typeof(TimeArg));
            return Expression.Lambda<Func<TimeArg, T>>(
                Expression.Invoke(input), tp);
        }

        public static Expression<Func<TimeArg, T>> ConvertExpression<T>(Expression<Func<DateTime, T>> input)
        {
            ParameterExpression tp = Expression.Parameter(typeof(TimeArg));
            return Expression.Lambda<Func<TimeArg, T>>(
                Expression.Invoke(input, Expression.Property(tp, "T")), tp);
        }

        public static Expression<Func<TimeArg, T>> ConvertExpression<T>(Expression<Func<DateTime, int, T>> input)
        {
            ParameterExpression tp = Expression.Parameter(typeof(TimeArg));
            return Expression.Lambda<Func<TimeArg, T>>(
                Expression.Invoke(input, Expression.Property(tp, "T"), Expression.Property(tp, "I")), tp);          
        }
    }
}
