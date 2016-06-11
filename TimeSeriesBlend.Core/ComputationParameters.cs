using System;
using System.Collections.Generic;

namespace TimeSeriesBlend.Core
{
    public class ComputationParameters
    {
        public DateTime From { get; set; }
        public DateTime Till { get; set; }
        public IEnumerable<string> VariablesToCompute { get; set; }
    }
}
