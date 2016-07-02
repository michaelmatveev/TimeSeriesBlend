using System;
using System.Collections.Generic;

namespace TimeSeriesBlend.Core
{
    public class ComputationParameters<I>
    {
        public I From { get; set; }
        public I Till { get; set; }
        public IEnumerable<string> VariablesToCompute { get; set; }
    }
}
