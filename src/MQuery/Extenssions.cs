using System;
using System.Linq;
using System.Reflection;
using MQuery.Filter;

namespace MQuery
{
    public static class Extenssions
    {
        public static void AddPropertyCompare(this FilterDocument filter, PropertyInfo propertyInfo, CompareOperator op, object value)
        {
            if(propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));
            if(propertyInfo.DeclaringType != filter.ElementType)
                throw new ArgumentException("The property is not declare in the target element type");

            filter.AddPropertyCompare(new PropertyNode(propertyInfo), new CompareNode(op, value));
        }
    }
}
