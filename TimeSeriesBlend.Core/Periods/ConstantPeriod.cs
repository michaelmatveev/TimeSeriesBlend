using System;
using System.Diagnostics;

namespace TimeSeriesBlend.Core.Periods
{
    internal class ConstantPeriod : CalculationPeriod
    {
        /// <summary>
        /// Константный период должен содержать только одно значение
        /// </summary>
        /// <param name="from"></param>
        /// <param name="till"></param>
        public override void GeneratePeriods(DateTime from, DateTime till)
        {
            Debug.Assert(_periods.Count == 0);
            _periods.Add(new DateTime());
        }

    }
}
