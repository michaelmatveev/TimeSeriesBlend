using System;
using System.Linq.Expressions;

namespace TimeSeriesBlend.Core
{
    internal static class ExpressionsBuilder<I>
    {
        public static Expression<Func<TimeArg<I>, T>> ConvertExpression<T>(Expression<Func<T>> input)
        {
            ParameterExpression tp = Expression.Parameter(typeof(TimeArg<I>));
            return Expression.Lambda<Func<TimeArg<I>, T>>(
                Expression.Invoke(input), tp);
        }

        public static Expression<Func<TimeArg<I>, T>> ConvertExpression<T>(Expression<Func<I, T>> input)
        {
            ParameterExpression tp = Expression.Parameter(typeof(TimeArg<I>));
            return Expression.Lambda<Func<TimeArg<I>, T>>(
                Expression.Invoke(input, Expression.Property(tp, "T")), tp);
        }

        public static Expression<Func<TimeArg<I>, T>> ConvertExpression<T>(Expression<Func<I, int, T>> input)
        {
            ParameterExpression tp = Expression.Parameter(typeof(TimeArg<I>));
            return Expression.Lambda<Func<TimeArg<I>, T>>(
                Expression.Invoke(input, Expression.Property(tp, "T"), Expression.Property(tp, "I")), tp);          
        }
    }
}
