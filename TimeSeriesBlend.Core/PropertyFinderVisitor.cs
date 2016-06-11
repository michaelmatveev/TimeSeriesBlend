using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace TimeSeriesBlend.Core
{
    /// <summary>
    /// Используется для нахождения зависимых переменных
    /// </summary>
    internal class PropertyFinderVisitor : ExpressionVisitor
    {
        private Type _HolderType;
        public List<PropertyInfo> DependedProperties { get; set; }

        public PropertyFinderVisitor(Type holderType)
        {
            _HolderType = holderType;
            DependedProperties = new List<PropertyInfo>();
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;
            if (node.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression me = (MemberExpression)node;
                if (me.Member.MemberType == MemberTypes.Property && me.Member.DeclaringType == _HolderType)
                {
                    DependedProperties.Add(me.Member as PropertyInfo);
                }
            }
            return base.Visit(node);
        }

    }
}
