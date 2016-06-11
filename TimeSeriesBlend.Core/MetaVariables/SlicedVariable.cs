using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesBlend.Core.MetaVariables
{
    /// <summary>
    /// Переменная не содержит Writer, формирует список значений из более короткого периода
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SlicedVariable<H, T> : CalculatedVariable<H, List<T>>
    {
        public PropertyInfo BasedOnMetaProperty { get; set; }

        public override void EvaluateInternal(H holder, MemberInfo groupKey)
        {
            // вычисляем переменную из котрой следует получить значения
            CalculatedVariable<H, T> basedVar = (CalculatedVariable<H, T>)DependsOn.Single();
            basedVar.Evaluate(holder, groupKey, LastMoniker);

            // для каждого периода времени вычисляем значение переменной
            foreach (var tp in Period.Periods.Select((t, i) => new TimeArg(t, i, groupKey, Period.Name, this.Name)))
            {
                DateTime next = Period.Periods.FirstOrDefault(p => p > tp.T);
                var subPeriod = basedVar.Period.Between(tp.T, next);
                List<T> result = Activator.CreateInstance<List<T>>();
                Int32 c = 0;
                foreach (DateTime s in subPeriod)
                {
                    var ta = new TimeArg(s, c++, groupKey, basedVar.Period.Name, basedVar.Name);
                    result.Add((T)basedVar.Results[ta]);
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

        protected override void FindDependentVariables(IEnumerable<MetaVariable<H>> allVariables)
        {
            DependsOn.Add(allVariables.Single(d => d.MetaProperty == this.BasedOnMetaProperty));
        }

    }
}
