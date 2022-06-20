using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery
{
    public interface IQuery<T>
    {
        IQueryable<T> ApplyTo(IQueryable<T> qy);
        IQueryable<T> FilterTo(IQueryable<T> qy);
        IQueryable<T> SliceTo(IQueryable<T> qy);
        IQueryable<T> SortTo(IQueryable<T> qy);
        Expression<Func<IQueryable<T>, IQueryable<T>>> GetQueryExpression();
        Expression<Func<IQueryable<T>, IQueryable<T>>> GetFilterExpression();
        Expression<Func<IQueryable<T>, IQueryable<T>>> GetSortExpression();
        Expression<Func<IQueryable<T>, IQueryable<T>>> GetSliceExpression();
    }
}