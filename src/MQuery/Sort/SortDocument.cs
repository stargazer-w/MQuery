using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MQuery.Sort
{
    public class SortDocument<T>
    {
        private readonly List<SortByPropertyNode> _sortByProperties = new();

        public IEnumerable<SortByPropertyNode> SortByPropertyNodes => _sortByProperties.AsReadOnly();

        public void AddSortByProperty<TProp>(Expression<Func<T, TProp>> selector, SortPattern pattern, int order = int.MaxValue)
        {
            AddSortByProperty(new(selector, pattern), order);
        }

        public void AddSortByProperty(SortByPropertyNode sortByPropertyNode, int order = int.MaxValue)
        {
            order = order switch
            {
                < 0 => 0,
                _ when order > _sortByProperties.Count => _sortByProperties.Count,
                _ => order,
            };

            _sortByProperties.Insert(order, sortByPropertyNode);
        }
    }
}
