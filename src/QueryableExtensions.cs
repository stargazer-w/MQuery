using System.Linq;

namespace MQuery
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> @this, QueryExpression<T> queryExpression)
        {
            return queryExpression.ExecuteWhere(@this);
        }

        public static IQueryable<T> Order<T>(this IQueryable<T> @this, QueryExpression<T> queryExpression)
        {
            return queryExpression.ExecuteOrder(@this);
        }

        public static IQueryable<T> Slice<T>(this IQueryable<T> @this, QueryExpression<T> queryExpression)
        {
            return queryExpression.ExecuteSlice(@this);
        }

        public static IQueryable<T> Slice<T>(this IQueryable<T> @this, QueryExpression<T> queryExpression, out int total)
        {
            return queryExpression.ExecuteSlice(@this, out total);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> @this, QueryExpression<T> queryExpression)
        {
            return queryExpression.ExecuteQuery(@this);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> @this, QueryExpression<T> queryExpression, out int total)
        {
            return queryExpression.ExecuteQuery(@this, out total);
        }
    }
}
