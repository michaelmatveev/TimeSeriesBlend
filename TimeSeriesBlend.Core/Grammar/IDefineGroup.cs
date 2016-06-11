using System;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IDefineGroup
    {
        IDefinePeriod BeginGroup(string caption, Func<object, System.Collections.IEnumerable> members);
        IDefinePeriod BeginGroup(string caption, Func<System.Collections.IEnumerable> members);    
    }
}
