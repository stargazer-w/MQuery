using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQuery.Sort
{
    public class SortDocument
    {
        private List<SortByPropertyNode> _sortByProperties = new List<SortByPropertyNode>();

        public Type ElementType { get; }

        public IEnumerable<SortByPropertyNode> SortByPropertyNodes => _sortByProperties.AsReadOnly();

        public SortDocument(Type elementType)
        {
            ElementType = elementType;
        }

        public void AddSortByProperty(PropertyNode propertyNode, SortType sortType)
        {
            if(propertyNode is null)
                throw new ArgumentNullException(nameof(propertyNode));

            var oldSort = _sortByProperties.FirstOrDefault(it => it.Property == propertyNode);
            if(oldSort != null)
                _sortByProperties.Remove(oldSort);
            _sortByProperties.Add(new SortByPropertyNode(propertyNode, sortType));
        }
    }
}
