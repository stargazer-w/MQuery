using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQuery.Sort
{
    public class SortDocument
    {
        private readonly List<SortByPropertyNode> _sortByProperties = new List<SortByPropertyNode>();

        public Type ElementType { get; }

        public IEnumerable<SortByPropertyNode> SortByPropertyNodes => _sortByProperties.AsReadOnly();

        public SortDocument(Type elementType)
        {
            ElementType = elementType;
        }

        public void AddSortByProperty(PropertyNode propertyNode, SortPattern sortType)
        {
            if(propertyNode is null)
                throw new ArgumentNullException(nameof(propertyNode));

            var oldSort = _sortByProperties.FirstOrDefault(it => it.Property.Equals(propertyNode));
            if(oldSort != null)
                _sortByProperties.Remove(oldSort);
            _sortByProperties.Add(new SortByPropertyNode(propertyNode, sortType));
        }
    }
}
