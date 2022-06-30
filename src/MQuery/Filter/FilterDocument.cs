using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class FilterDocument<T>
    {
        static readonly Expression<Func<T, bool>> _alwaysTrue = x => true;

        public IParameterOperation Operation { get; set; } = new And();

        public Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression()
        {
            var body = Operation.Combine(_alwaysTrue.Parameters[0]);
            if(body is ConstantExpression { Value: true })
                return x => x;
            var predicate = _alwaysTrue.Update(body, _alwaysTrue.Parameters);
            return x => x.Where(predicate);
        }

        public IQueryable<T> ApplyTo(IQueryable<T> source)
        {
            var body = Operation.Combine(_alwaysTrue.Parameters[0]);
            if(body is ConstantExpression { Value: true })
                return source;
            var predicate = _alwaysTrue.Update(body, _alwaysTrue.Parameters);
            return source.Where(predicate);
        }
    }
}
