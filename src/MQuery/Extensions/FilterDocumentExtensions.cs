using System;
using System.Linq.Expressions;
using MQuery.Filter;

namespace MQuery.Extensions
{
    public static class FilterDocumentExtensions
    {
        public static FilterDocument<T> And<T, U>(this FilterDocument<T> filter, Expression<Func<T, U>> selector, IOperator @operator)
        {
            var operation = new PropertyOperation<T>(new PropertyLambda<T>(selector), @operator);
            if(filter.Operation is And and)
            {
                and.Operators.Add(operation);
            }
            else
            {
                filter.Operation = new And(filter.Operation, operation);
            }
            return filter;
        }
    }
}
