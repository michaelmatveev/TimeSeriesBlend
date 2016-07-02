using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TimeSeriesBlend.Core.Periods
{
    internal class CalculationPeriod<I>
    {
        public string Name { get; set; }

        public GroupOfVariables WithinGroup { get; set; }

        /// <summary>
        /// Function to return next period of time
        /// Функция, которая используется для вычисления следующего интервала времени 
        /// </summary>
        /// <returns></returns>
        public Func<I, I> GetNextPeriod { get; set; }

        protected readonly IList<I> _periods;

        public IEnumerable<I> Periods
        {
            get { return _periods; }
        }

        public CalculationPeriod()
        {
            _periods = new List<I>();
        }

        /// <summary>
        /// Вычисляет все периоды времени между from и till
        /// </summary>
        /// <param name="from">Начальный момент времени с которого начинается отсчет периода</param>
        /// <param name="till">Момент времени следюущий после последнего периода</param>
        public virtual void GeneratePeriods(I from, I till)
        {
            Debug.Assert(!_periods.Any());
            CheckInputInterval(from, till);

            while (Operator.LessThan(from, till))
            {
                _periods.Add(from);
                from = GetNextPeriod(from);
            };
        }

        /// <summary>
        /// Возващает периоды времени между from и till
        /// </summary>
        /// <param name="from"></param>
        /// <param name="till"></param>
        /// <returns></returns>
        public IEnumerable<I> Between(I from, I till)
        {
            CheckPeriods();            
            if (Operator.Equal(till, default(I)))
            {
                return _periods.Where(t => Operator.GreaterThanOrEqual(t, from));
            }

            CheckInputInterval(from, till);
            return _periods.Where(t => Operator.GreaterThanOrEqual(t, from) && Operator.LessThan(t, till));
        }

        private void CheckInputInterval(I from, I till)
        {
            if (Operator.GreaterThan(from, till))
            {
                var exMessage = $"\"till\" parameter value = {till} must be greather then value of \"from\" patmater = {from}.";
                throw new ArgumentException(exMessage, "till");
            }
        }

        private void CheckPeriods()
        {
            if (!_periods.Any())
            {
                var exMessage = @"Periods are not filled. Call /""GeneratePeriods/"" before.";
                throw new InvalidOperationException(exMessage);
            }
        }

    }
}

