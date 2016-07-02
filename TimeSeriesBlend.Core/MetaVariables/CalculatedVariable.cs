using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TimeSeriesBlend.Core.Periods;

namespace TimeSeriesBlend.Core.MetaVariables
{
    internal class CalculatedVariable<H, T, I> : MetaVariable<H, I>
    {
        /// <summary>
        /// Сохраненные результаты вычислений данной переменной на каждый период
        /// Заполняется по мере проведения вычислений в методе Evaluate
        /// </summary>
        /// <returns></returns>
        public Dictionary<TimeArg<I>, T> Results { get; private set; }

        public Expression<Func<TimeArg<I>, T>> Writer { get; set; }
        public Func<TimeArg<I>, T> CompiledWriter { get; set; }

        protected Action<H, T> CompiledPropertySetter { get; set; }

        private static PropertyFinderVisitor visitor = new PropertyFinderVisitor(typeof(H));

        public CalculatedVariable()
        {
            Results = new Dictionary<TimeArg<I>, T>(TimeArgsComparer<I>.Instance);
        }

        protected override void ValidateVariable()
        {
            if (Writer == null)
            {
                throw new InvalidOperationException(String.Format("Need to enter expression to evaluate variable {0}.", Name));
            }

            if (MetaProperty == null)
            {
                throw new InvalidOperationException("Meta property is not defined");
            }
        }

        protected override void FindDependentVariables(IEnumerable<MetaVariable<H, I>> allVariables)
        {
            visitor.DependedProperties.Clear();
            visitor.Visit(Writer.Body);

            // добавляем все переменные связанные со свойствами, упоминаемыми в Writer.Body
            DependsOn.AddRange(allVariables.Where(d => visitor.DependedProperties.Contains(d.MetaProperty)));
        }

        protected override void CompileInternal()
        {
            if (Writer != null)
            {
                CompiledWriter = Writer.Compile();
            }

            ParameterExpression paramExpression = Expression.Parameter(typeof(H));
            ParameterExpression paramExpression2 = Expression.Parameter(typeof(T), MetaProperty.Name);
            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, MetaProperty.Name);

            CompiledPropertySetter = Expression.Lambda<Action<H, T>>(
                Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2
            ).Compile();
        }

        public override void EvaluateInternal(H holder, MemberInfo groupKey)
        {
            // вычисляем все переменные, от которой зависит данная переменная
            foreach (MetaVariable<H, I> vd in DependsOn)
            {
                vd.Evaluate(holder, groupKey, LastMoniker);
            }

            // для каждого периода времени вычисляем значение переменной
            foreach (var tp in Period.Periods.Select((t, i) => new TimeArg<I>(t, i, groupKey, Period.Name, this.Name)))
            {
                T result;
                // выставляем в holder значения мета-переменных от которых зависит данная переменная
                foreach (MetaVariable<H, I> vd in DependsOn)
                {
                    vd.ApplyValueForHolder(holder, tp);
                }
                // вычисляем новое значение и заносим его в Results
                result = CompiledWriter(tp);
                Results.Add(tp, result);
                // если требуется вернуть значение переменной, то записываем результат в мета-переменную
                // holder содержить набор значений всех переменных на момент времени t
                if (Readers.Any())
                {
                    ApplyValueForHolder(holder, tp);
                    ExecuteAllReaders(tp);
                }
            }
        }

        public override void ApplyValueForHolder(H holder, TimeArg<I> tp)
        {
            T result;
            if (this.Period is ConstantPeriod<I>)
            {
                result = Results.Values.Single();
            }
            else
            {
                result = Results[tp];
            }
            CompiledPropertySetter(holder, result);
        }

    }
}
