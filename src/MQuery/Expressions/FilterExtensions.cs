using MQuery.Filter;

namespace MQuery.Expressions
{
    public static class FilterExtensions
    {
        public static FilterDocument<T> And<T>(this FilterDocument<T> @this, IParameterOperation operation)
        {
            if(@this.Operation is And and)
            {
                and.Operators.Add(operation);
            }
            else
            {
                and = new And(@this.Operation, operation);
                @this.Operation = and;
            }
            return @this;
        }
    }
}
