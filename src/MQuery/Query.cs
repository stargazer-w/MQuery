using MQuery.Expressions;
using System.Linq;

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
            var expr = Document.ToExpression();
            return expr.Compile()(qy);
        }

        public IQueryable<T> FilterTo(IQueryable<T> qy)
        {
            var expr = Document.Filter.ToExpression();
            return expr.Compile()(qy);
        }

        public IQueryable<T> SortTo(IQueryable<T> qy)
        {
            var expr = Document.Sort.ToExpression();
            return expr.Compile()(qy);
        }
        public IQueryable<T> SliceTo(IQueryable<T> qy)
        {
            var expr = Document.Slice.ToExpression<T>();
            return expr.Compile()(qy);
        }
    }
}
