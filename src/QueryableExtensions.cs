using System.Linq;

namespace MQuery
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.WhereFrom(@this);
        }

        public static IQueryable<T> Order<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.Order(@this);
        }

        public static IQueryable<T> GetPage<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.GetPageFrom(@this);
        }

        public static IQueryable<T> GetPage<T>(this IQueryable<T> @this, Query<T> query, out int total)
        {
            return query.GetPageFrom(@this, out total);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.QueryFrom(@this);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> @this, Query<T> query, out int total)
        {
            return query.QueryFrom(@this, out total);
        }
    }
}
