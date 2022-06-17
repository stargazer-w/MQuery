using MQuery.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery
{
    public class Query<T>
    {
        public QueryDocument<T> Document { get; }

        public Query()
        {
            Document = new QueryDocument<T>();
        }

        public IQueryable<T> ApplyTo(IQueryable<T> qy)
        {
            var expr = (Expression<Func<IQueryable<T>, IQueryable<T>>>)Document.ToExpression();
            return expr.Compile()(qy);
        }

        public IQueryable<T> FilterTo(IQueryable<T> qy)
        {
            var expr = (Expression<Func<IQueryable<T>, IQueryable<T>>>)Document.Filter.ToExpression();
            return expr.Compile()(qy);
        }

        public IQueryable<T> SortTo(IQueryable<T> qy)
        {
            var expr = (Expression<Func<IQueryable<T>, IQueryable<T>>>)Document.Sort.ToExpression();
            return expr.Compile()(qy);
        }
        public IQueryable<T> SliceTo(IQueryable<T> qy)
        {
            var expr = Document.Slicing.ToExpression<T>();
            return expr.Compile()(qy);
        }
    }
}
