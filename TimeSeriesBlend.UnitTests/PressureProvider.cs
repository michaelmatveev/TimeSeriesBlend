using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesBlend.UnitTests
{
    public class PressureProvider
    {
        public static int GetPressure(DateTime d)
        {
            return (int)d.DayOfWeek;
        }
            
    }
}
