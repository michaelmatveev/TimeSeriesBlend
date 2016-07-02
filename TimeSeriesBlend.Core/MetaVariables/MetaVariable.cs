using System;
using System.Collections.Generic;
using System.Reflection;
using TimeSeriesBlend.Core.Periods;

namespace TimeSeriesBlend.Core.MetaVariables
{
    internal enum CompilationState
    {
        NotCompiled,
        Compiled
    }

    internal abstract class MetaVariable<H, I>
    {
        public string Name { get; set; }

        /// <summary>
        /// Список переменных, которые должны быть вычислены перед вычислением данной переменной
        /// Этот список заполняется на этапе компиляции выражения
        /// </summary>
        public List<MetaVariable<H, I>> DependsOn { get; set; }

        public CalculationPeriod<I> Period { get; set; }

        public GroupOfVariables Group { get; set; }

        public bool RequiredCalculation { get; set; }

        public IList<Action<TimeArg<I>>> Readers { get; set; }

        /// <summary>
        /// Свойство из holder связанное с данной мета-переменной
        /// </summary>
        public PropertyInfo MetaProperty { get; set; }

        protected Guid LastMoniker { get; set; }

        public CompilationState State { get; private set; }

        public MetaVariable()
        {
            DependsOn = new List<MetaVariable<H, I>>();
            State = CompilationState.NotCompiled;
            Readers = new List<Action<TimeArg<I>>>();
        }

        protected void ExecuteAllReaders(TimeArg<I> arg)
        {
            foreach (var reader in Readers)
            {
                reader(arg);
            }
        }
        
        protected virtual void ValidateVariable()
        {
        }

        protected abstract void FindDependentVariables(IEnumerable<MetaVariable<H, I>> allVariables);

        protected virtual void CompileInternal()
        {
        }

        /// <summary>
        /// Проверить состояние переменной, найти зависимые пеменные, скомпилировать при необходимости
        /// </summary>
        /// <param name="allVariables"></param>
        public void Compile(IEnumerable<MetaVariable<H, I>> allVariables)
        {
            ValidateVariable();
            FindDependentVariables(allVariables);
            CompileInternal();
            State = CompilationState.Compiled;
        }

        public void Evaluate(H holder, MemberInfo groupMember, Guid moniker)
        {
            // не вычисляем переменную, если она уже была вычислена
            // вычисляем, если переменная не вычислялась ни разу или пришел новый моникер (новое значение moniker)        
            if (LastMoniker == Guid.Empty || moniker != LastMoniker)
            {
                LastMoniker = moniker;
                EvaluateInternal(holder, groupMember);
            }
        }

        public abstract void EvaluateInternal(H holder, MemberInfo groupMember);

        public virtual void ApplyValueForHolder(H holder, TimeArg<I> tp)
        {
        }
    }

}
