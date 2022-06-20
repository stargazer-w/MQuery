using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class FilterDocument<T>
    {
        private readonly List<IPropertyComparesNode> _propertyCompares = new();

        public IEnumerable<IPropertyComparesNode> PropertyCompares => _propertyCompares.AsReadOnly();

        public void AddPropertyCompare<TProp, TValue>(Expression<Func<T, TProp>> selector, CompareOperator @operator, TValue value)
        {
            AddPropertyCompare(new PropertyComparesNode<TValue>(selector, @operator, value));
        }

        public void AddPropertyCompare(IPropertyComparesNode propertyComparesNode)
        {
            _propertyCompares.Add(propertyComparesNode);
        }
    }
}
