using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesBlend.UnitTests
{
    internal class TemperatureProvider
    {
        public static int GetTemperature(DateTime date)
        {
            return date.Hour / 2;
        }
    }
}
