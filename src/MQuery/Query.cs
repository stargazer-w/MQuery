using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery
{
    public class Query<T> : IQuery<T>
    {
        public QueryDocument<T> Document { get; } = new();

        public IQueryable<T> ApplyTo(IQueryable<T> qy)
        {
            return GetQueryExpression().Compile()(qy);
        }

        public IQueryable<T> FilterTo(IQueryable<T> qy)
        {
            return GetFilterExpression().Compile()(qy);
        }

        public IQueryable<T> SortTo(IQueryable<T> qy)
        {
            return GetSortExpression().Compile()(qy);
        }
        public IQueryable<T> SliceTo(IQueryable<T> qy)
        {
            return GetSliceExpression().Compile()(qy);
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> GetQueryExpression()
        {
            return Document.ToExpression();
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> GetFilterExpression()
        {
            return Document.Filter.ToExpression();
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> GetSortExpression()
        {
            return Document.Sort.ToExpression();
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> GetSliceExpression()
        {
            return Document.Slice.ToExpression<T>();
        }
    }
}
