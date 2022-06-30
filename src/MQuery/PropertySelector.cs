using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery
{
    public class PropertySelector
    {
        public IEnumerable<string> PropertyNames { get; }
        public Type LeftType { get; }
        public Type PropType { get; }

        public PropertySelector(Type type, params string[] propertyNames)
        {
            PropType = LeftType = type;
            for(int i = 0; i < propertyNames.Length; i++)
            {
                var name = propertyNames[i];
                var p = PropType.GetProperty(name);
                if(p is null)
                {
                    var nameChain = string.Join(".", propertyNames.Take(i + 1));
                    throw new ArgumentException($"can not found property {nameChain} of object");
                }
                PropType = p.PropertyType;
            }

            PropertyNames = propertyNames;
        }

        public Expression ToExpression(ParameterExpression left)
        {
            if(left.Type != LeftType)
                throw new ArgumentException("left expression's type must equals LeftType");

            Expression selector = left;
            foreach(var prop in PropertyNames)
            {
                selector = Expression.Property(selector, prop);
            }
            return selector;
        }
    }
}
