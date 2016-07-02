using System;

namespace TimeSeriesBlend.Core.Grammar
{
    public interface IDefineGroup<I>
    {
        IDefinePeriod<I> BeginGroup(string caption, Func<object, System.Collections.IEnumerable> members);
        IDefinePeriod<I> BeginGroup(string caption, Func<System.Collections.IEnumerable> members);    
    }
}
