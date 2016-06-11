using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeSeriesBlend.Core.MetaVariables
{
    /// <summary>
    /// Не содержит собственного значения. Используется только для того, чтобы прочитать значения других переменных, вычисленных для данного периода.
    /// </summary>
    internal class SummarizeVariable<H> : MetaVariable<H>
    {
        public SummarizeVariable(Action<TimeArg> action) : base()
        {
            Readers.Add(action);
        }

        public override void EvaluateInternal(H holder, MemberInfo groupKey)
        {
            // вычисляем все переменные, от которой зависит данныая переменная
            foreach (MetaVariable<H> vd in DependsOn)
            {
                vd.Evaluate(holder, groupKey, LastMoniker);
            }
            // для каждого периода времени вычисляем значение переменной
            foreach (var tp in Period.Periods.Select((t, i) => new TimeArg(t, i, groupKey, Period.Name, this.Name)))
            {
                // выставляем в holder значения мета-переменных от которых зависит данная переменная
                foreach (MetaVariable<H> vd in DependsOn)
                {
                    vd.ApplyValueForHolder(holder, tp);
                }
                // holder содержит набор значений всех переменных на момент времени t
                ExecuteAllReaders(tp);
            }
        }

        protected override void FindDependentVariables(IEnumerable<MetaVariable<H>> allVariables)
        {
            DependsOn.AddRange(allVariables.Where(d => d.Period == Period && d != this));
        }

    }
}
