using System;
using System.Collections.Generic;
using MQuery.Filter;

namespace MQuery.Builders
{
    public class FilterBuilder<T>
    {
        internal ConditionBuilder<T>? _conditionBuilder;


        public ConditionBuilder<T> Prop(params string[] props)
        {
            var _propSelector = new PropertySelector(typeof(T), props);
            return new ConditionBuilder<T>(_propSelector, this);
        }

        public FilterDocument<T> Build()
        {
            if(_conditionBuilder != null)
                return new FilterDocument<T> { Operation = _conditionBuilder.CreateQueryProperty() };
            return new();
        }
    }

    public class ConditionBuilder<T>
    {
        private readonly FilterBuilder<T> _filterBuilder;
        private readonly PropertySelector _propSelector;
        private Func<IOperator, IParameterOperation> _createQueryProperty;
        private IOperator? _op;

        public ConditionBuilder(PropertySelector propSelector, FilterBuilder<T> filterBuilder)
        {
            _propSelector = propSelector;
            _filterBuilder = filterBuilder;
            _createQueryProperty = op => new QueryProperty(_propSelector, op);
        }

        public FilterBuilder<T> Eq<U>(U value)
        {
            _op = new Equal<U>(value);
            _filterBuilder._conditionBuilder = this;
            return _filterBuilder;
        }

        public FilterBuilder<T> Gt<U>(U? value)
        {
            _op = new GreaterThen<U>(value);
            _filterBuilder._conditionBuilder = this;
            return _filterBuilder;
        }

        public FilterBuilder<T> In<U>(IEnumerable<U?> value)
        {
            _op = new In<U>(value);
            _filterBuilder._conditionBuilder = this;
            return _filterBuilder;
        }

        public ConditionBuilder<T> Not()
        {
            var old = _createQueryProperty;
            _createQueryProperty = op => new Not(old(op));
            return this;
        }

        internal IParameterOperation CreateQueryProperty()
        {
            return _createQueryProperty(_op!);
        }
    }
}
