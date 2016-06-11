using System;
using System.Collections.Generic;

namespace TimeSeriesBlend.Core
{
    internal class GroupOfVariables
    {
        public string Name { get; set; }        
        public GroupOfVariables Parent { get; set; }
        public IList<GroupOfVariables> Children { get; private set; }
        public Func<object, System.Collections.IEnumerable> MembersGenerator { get; set; }
        private readonly IList<MemberInfo> _members = new List<MemberInfo>();

        public GroupOfVariables()
        {
            Children = new List<GroupOfVariables>();
        }

        public int GetLevel()
        {
            return Parent == null ? 0 : Parent.GetLevel() + 1;
        }        

        public IEnumerable<MemberInfo> Members
        {
            get
            {
                return _members;
            }
        }

        public void GetMembers(MemberInfo parent)
        {
            foreach (var m in MembersGenerator(parent.Value))
            {
                var newMember = new MemberInfo
                {
                    Value = m,
                    ParentMember = parent
                };
                _members.Add(newMember);
                foreach (var c in Children)
                {
                    c.GetMembers(newMember);
                }
            }
        }

    }
}
