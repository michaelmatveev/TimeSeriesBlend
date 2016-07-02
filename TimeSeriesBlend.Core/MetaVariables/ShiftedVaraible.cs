using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TimeSeriesBlend.Core.MetaVariables
{
    internal class ShiftedVaraible<H, T, I> : CalculatedVariable<H, T, I>
    {
        public PropertyInfo BasedOnMetaProperty { get; set; }
        public Int32 Shift { get; set; }
        public Func<TimeArg<I>, T> EmptyFiller { get; set; }

        public override void EvaluateInternal(H holder, MemberInfo groupKey)
        {
            // вычисляем переменную из котрой следует получить значения
            CalculatedVariable<H, T, I> basedVar = (CalculatedVariable<H, T, I>)DependsOn.Single();

            // check basedVar.Period == this.Period; (они должны совпадать)
            basedVar.Evaluate(holder, groupKey, LastMoniker);

            var periods = Period.Periods.Select((t, i) => new TimeArg<I>(t, i, groupKey, Period.Name, this.Name));
            if (Shift >= 0)
            {
                ShiftData(basedVar, periods, Shift);
            }
            else
            {
                var p2 = Period.Periods.Reverse().Select((t, i) => new TimeArg<I>(t, i, groupKey, Period.Name, this.Name));
                ShiftData(basedVar, p2, -Shift);
            }

            foreach (var tp in periods)
            {
                if (Readers.Any())
                {
                    ApplyValueForHolder(holder, tp);
                    ExecuteAllReaders(tp);
                }
            }
        }

        private void ShiftData(CalculatedVariable<H, T, I> basedVar, IEnumerable<TimeArg<I>> periods, Int32 shift)
        {
            Dictionary<TimeArg<I>, T> temp = new Dictionary<TimeArg<I>, T>();
            Queue<T> buffer = new Queue<T>(shift);
            T result;
            foreach (var tp in periods)
            {
                buffer.Enqueue(basedVar.Results[tp]);
                if (tp.I < shift)
                {
                    result = EmptyFiller(tp);
                }
                else
                {
                    result = buffer.Dequeue();
                }
                Results.Add(tp, result);
            }
        }

        protected override void ValidateVariable()
        {
            if (MetaProperty == null)
            {
                throw new InvalidOperationException("Meta property is not defined");
            }
        }

        protected override void FindDependentVariables(IEnumerable<MetaVariable<H, I>> allVariables)
        {
            DependsOn.Add(allVariables.Single(d => d.MetaProperty == this.BasedOnMetaProperty));
        }

    }
}
