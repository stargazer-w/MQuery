using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MQuery
{
    public class PropertySelector<T> : IPropertySelector<T>
    {
        public IEnumerable<string> PropertyNames { get; }
        public Type PropertyType { get; }
        public Type? PropertyCollectionElementType { get; }

        public PropertySelector(params string[] propertyNames)
        {
            PropertyType = typeof(T);
            for(int i = 0; i < propertyNames.Length; i++)
            {
                var name = propertyNames[i];
                var p = PropertyType.GetProperty(name);
                if(p is null)
                {
                    throw new ArgumentException($"can not found property {name} of {(i > 0 ? propertyNames[i - 1] : "$")}");
                }
                PropertyType = p.PropertyType;
            }

            if(IsCollection(PropertyType, out var eleType))
            {
                PropertyCollectionElementType = eleType;
            }

            PropertyNames = propertyNames;
        }

        public Expression ToExpression(ParameterExpression left)
        {
            if(left.Type != typeof(T))
                throw new ArgumentException("left expression's type must be equal to LeftType");

            Expression selector = left;
            foreach(var prop in PropertyNames)
            {
                selector = Expression.Property(selector, prop);
            }
            return selector;
        }

        static bool IsCollection(Type type, out Type? elementType)
        {
            if(type.GetInterface("ICollection`1")?.GetGenericArguments()[0] is Type eleType)
            {
                elementType = eleType;
                return true;
            }
            else
            {
                elementType = null;
                return false;
            }
        }
    }
}
