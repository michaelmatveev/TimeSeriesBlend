using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TimeSeriesBlend.Core.Periods
{
    internal class CalculationPeriod
    {
        public string Name { get; set; }

        public GroupOfVariables WithinGroup { get; set; }

        /// <summary>
        /// Function to return next period of time
        /// Функция, которая используется для вычисления следующего интервала времени 
        /// </summary>
        /// <returns></returns>
        public Func<DateTime, DateTime> GetNextPeriod { get; set; }

        protected readonly IList<DateTime> _periods;

        public IEnumerable<DateTime> Periods
        {
            get { return _periods; }
        }

        public CalculationPeriod()
        {
            _periods = new List<DateTime>();
        }

        /// <summary>
        /// Вычисляет все периоды времени между from и till
        /// </summary>
        /// <param name="from">Начальный момент времени с которого начинается отсчет периода</param>
        /// <param name="till">Момент времени следюущий после последнего периода</param>
        public virtual void GeneratePeriods(DateTime from, DateTime till)
        {
            Debug.Assert(!_periods.Any());
            CheckInputInterval(from, till);

            while (from < till)
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
        public IEnumerable<DateTime> Between(DateTime from, DateTime till)
        {
            CheckPeriods();            
            if (till == default(DateTime))
            {
                return _periods.Where(t => t >= from);
            }

            CheckInputInterval(from, till);
            return _periods.Where(t => t >= from && t < till);
        }

        private void CheckInputInterval(DateTime from, DateTime till)
        {
            if (from > till)
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

