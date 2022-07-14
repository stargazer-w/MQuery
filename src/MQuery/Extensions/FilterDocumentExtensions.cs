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
            return filter.And(operation);
        }

        public static FilterDocument<T> And<T>(this FilterDocument<T> filter, Expression<Func<T, bool>> expression)
        {
            var operation = new LambdaOperation<T>(expression);
            return filter.And(operation);
        }

        public static FilterDocument<T> And<T>(this FilterDocument<T> filter, IParameterOperation operation)
        {
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

        public static FilterDocument<T> Or<T, U>(this FilterDocument<T> filter, Expression<Func<T, U>> selector, IOperator @operator)
        {
            var operation = new PropertyOperation<T>(new PropertyLambda<T>(selector), @operator);
            return filter.Or(operation);
        }

        public static FilterDocument<T> Or<T>(this FilterDocument<T> filter, Expression<Func<T, bool>> expression)
        {
            var operation = new LambdaOperation<T>(expression);
            return filter.Or(operation);
        }

        public static FilterDocument<T> Or<T>(this FilterDocument<T> filter, IParameterOperation operation)
        {
            if(filter.Operation is Or or)
            {
                or.Operations.Add(operation);
            }
            else
            {
                filter.Operation = new And(filter.Operation, operation);
            }
            return filter;
        }
    }
}
