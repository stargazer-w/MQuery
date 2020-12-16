using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MQuery.Expressions
{
    public static class QueryExpressionExtenssions
    {
        public static LambdaExpression ToExpression(this QueryDocument query)
        {
            var filterExpr = query.Filter.ToExpression();
            var sortExpr = query.Sort.ToExpression();
            var slicingExpr = query.Slicing.ToExpression();

            var filterThenSort = Expression.Invoke(sortExpr, Expression.Invoke(filterExpr, filterExpr.Parameters));
            var filterThenSortThenSlicing = Expression.Invoke(slicingExpr, filterThenSort);
            return Expression.Lambda(filterThenSortThenSlicing, filterExpr.Parameters);
        }
    }
}
