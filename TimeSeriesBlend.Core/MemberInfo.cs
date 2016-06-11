using System.Collections.Generic;
using System.Linq;

namespace TimeSeriesBlend.Core
{
    public class MemberInfo
    {
        public object Value { get; set; }
        public MemberInfo ParentMember { get; set; }

        public IEnumerable<MemberInfo> Parents
        {
            get
            {
                if (ParentMember == null)
                {
                    return new MemberInfo[] { };
                }
                else
                {
                    return (new[] { ParentMember }).Union(ParentMember.Parents);
                }
            }
        }

    }
}
