using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class FilterDocument<T>
    {
        private readonly List<IPropertyComparesNode> _propertyCompares = new();

        public IEnumerable<IPropertyComparesNode> PropertyCompares => _propertyCompares.AsReadOnly();

        public void AddPropertyCompare<TProp, TValue>(Expression<Func<T, TProp>> selector, CompareOperator compare, TValue prop)
        {
            if(selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            _propertyCompares.Add(new PropertyComparesNode<TValue>(selector, compare, prop));
        }
    }
}
