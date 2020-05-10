using System.Linq;

namespace MQuery
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.Filter(@this);
        }

        public static IQueryable<T> Order<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.Order(@this);
        }

        public static IQueryable<T> Slice<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.Slice(@this);
        }

        public static IQueryable<T> Slice<T>(this IQueryable<T> @this, Query<T> query, out int total)
        {
            return query.Slice(@this, out total);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> @this, Query<T> query)
        {
            return query.Execute(@this);
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> @this, Query<T> query, out int total)
        {
            return query.Execute(@this, out total);
        }
    }
}
