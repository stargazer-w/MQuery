using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MQuery.Filter
{
    public class FilterDocument
    {
        private readonly List<PropertyComparesNode> _propertyCompares = new List<PropertyComparesNode>();

        public Type ElementType { get; }

        public IEnumerable<PropertyComparesNode> PropertyCompares => _propertyCompares.AsReadOnly();

        public FilterDocument(Type eleType)
        {
            ElementType = eleType;
        }

        public void AddPropertyCompare(PropertyNode property, CompareNode compare, params CompareNode[] otherCompares)
        {
            if(property is null)
                throw new ArgumentNullException(nameof(property));

            var cmp = _propertyCompares.FirstOrDefault(it => it.Property == property);
            if(cmp == null)
            {
                cmp = new PropertyComparesNode(property, compare, otherCompares);
                _propertyCompares.Add(cmp);
            }
            else
            {
                foreach(var otherCompare in otherCompares.Prepend(compare))
                {
                    cmp.AddCompare(otherCompare);
                }
            }
        }
    }
}
