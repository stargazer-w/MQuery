using System.Linq.Expressions;

namespace MQuery.Expressions
{
    public static class QueryExpressionExtenssions
    {
        public static LambdaExpression ToExpression<T>(this QueryDocument<T> query)
        {
            var filterExpr = query.Filter.ToExpression();
            var sortExpr = query.Sort.ToExpression();
            var slicingExpr = query.Slicing.ToExpression<T>();

            var filterThenSort = Expression.Invoke(sortExpr, Expression.Invoke(filterExpr, filterExpr.Parameters));
            var filterThenSortThenSlicing = Expression.Invoke(slicingExpr, filterThenSort);
            return Expression.Lambda(filterThenSortThenSlicing, filterExpr.Parameters);
        }
    }
}
