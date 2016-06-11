using System;
using System.Collections.Generic;

namespace TimeSeriesBlend.Core
{
    /// <summary>
    /// Аргумент временного ряда
    /// </summary>
    public struct TimeArg
    {
        public DateTime T { get; private set; }
        public int I { get; private set; }
        public MemberInfo ForGroupMember { get; private set; }
        public string CurrentPeriodName { get; private set; }
        public string CurrentVariableName { get; private set; }
        
        public TimeArg(DateTime t, int i, MemberInfo member, string periodName, string variableName)
        {
            T = t;
            I = i;
            ForGroupMember = member;
            CurrentPeriodName = periodName;
            CurrentVariableName = variableName;
        }
        
        public object GroupKey
        {
            get
            {
                return ForGroupMember.Value;
            }
        }

    }

    /// <summary>
    /// Временные аргументы должны различаться только временем и ключом группы, все прочие свойства должны игнорироваться при сравнении
    /// </summary>
    internal class TimeArgsComparer : IEqualityComparer<TimeArg>
    {
        static TimeArgsComparer()
        {
            Instance = new TimeArgsComparer();
        }

        public static TimeArgsComparer Instance { get; private set; }
        
        public bool Equals(TimeArg x, TimeArg y)
        {
            return (x.T == y.T) && (x.ForGroupMember == y.ForGroupMember);
        }

        public int GetHashCode(TimeArg obj)
        {
            if (obj.ForGroupMember == null)
            {
                return obj.T.GetHashCode();
            }
            int hash = 17;
            hash = hash * 23 + obj.T.GetHashCode();
            hash = hash * 31 + obj.ForGroupMember.GetHashCode();
            return hash;
        }
    }
}
