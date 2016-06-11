using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesBlend.UnitTests
{
    public class TempPress
    {
        public const double C = 2;

        public double Result { get; set; }
        public int TemperatureInCelsius { get; set; }
        public int TemperatureAbs { get; set; }
        public int Pressure { get; set; }
    }
}
