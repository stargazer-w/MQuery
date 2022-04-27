using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MQuery.Filter
{
    public class PropertyComparesNode
    {
        private readonly List<ICompareNode> _compares;

        public PropertyNode Property{ get; }

        public IEnumerable<ICompareNode> Compares => _compares.AsReadOnly();

        public PropertyComparesNode(PropertyNode property, ICompareNode compare, params ICompareNode[] otherCompares)
        {
            if(otherCompares is null)
                throw new ArgumentNullException(nameof(otherCompares));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            _compares = otherCompares.Prepend(compare).ToList();
        }

        public void AddCompare(ICompareNode compareNode)
        {
            _compares.Add(compareNode);
        }
    }
}
