using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TimeSeriesBlend.Core.MetaVariables
{
    /// <summary>
    /// Переменная не содержит Writer, формирует список значений из более короткого периода
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CrossGroupVariable<H, K, T, I> : CalculatedVariable<H, Dictionary<K, T>, I>
    {
        public PropertyInfo BasedOnMetaProperty { get; set; }

        public override void EvaluateInternal(H holder, MemberInfo groupKey)
        {
            // вычисляем переменную из котрой следует получить значения
            CalculatedVariable<H, T, I> basedVar = (CalculatedVariable<H, T, I>)DependsOn.Single();
            // вычислять не требуется, т.к. переменная должна быть уже вычислена в более глубокой группе

            // для каждого периода времени вычисляем значение переменной
            foreach (var tp in Period.Periods.Select((t, i) => new TimeArg<I>(t, i, groupKey, Period.Name, this.Name)))
            {
                Dictionary<K, T> result = Activator.CreateInstance<Dictionary<K, T>>();
                //foreach (var p in basedVar.Results.Where(pair => pair.Key.T == tp.T && pair.Key.ForMember.ParentMember == groupKey))
                foreach (var p in basedVar.Results.Where(pair => Operator.Equal(pair.Key.T, tp.T) && pair.Key.ForGroupMember.Parents.Contains(groupKey)))
                {
                    result.Add((K)p.Key.GroupKey, p.Value);
                }

                Results.Add(tp, result);
                // если требуется вернуть значение переменной, то записываем результат в свойство
                // holder, это свойство будет содержить список значений всех переменных на момент времени t
                if (Readers.Any())
                {
                    ApplyValueForHolder(holder, tp);
                    ExecuteAllReaders(tp);
                }
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
