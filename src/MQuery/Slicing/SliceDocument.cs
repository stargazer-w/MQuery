using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Slice
{
    public class SliceDocument
    {
        public int? Skip { get; set; }

        public int? Limit { get; set; }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression<T>()
        {
            return this switch
            {
                { Skip: null, Limit: null } => qy => qy,
                { Skip: null, Limit: int limit } => qy => qy.Take(limit),
                { Skip: int skip, Limit: null } => qy => qy.Skip(skip),
                { Skip: int skip, Limit: int limit } => qy => qy.Skip(skip).Take(limit),
            };
        }
    }
}
