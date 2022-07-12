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
            var _propSelector = new PropertySelector<T>(props);
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
        private readonly PropertySelector<T> _propSelector;
        private Func<IOperator, IOperator> _opBuilder = x => x;
        private readonly Func<IOperator, IParameterOperation> _createQueryProperty;
        private IOperator? _op;

        public ConditionBuilder(PropertySelector<T> propSelector, FilterBuilder<T> filterBuilder)
        {
            _propSelector = propSelector;
            _filterBuilder = filterBuilder;
            _createQueryProperty = op => new PropertyOperation<T>(_propSelector, op);
        }

        public FilterBuilder<T> Eq<U>(U value)
        {
            _op = _opBuilder(new Equal<U>(value));
            _filterBuilder._conditionBuilder = this;
            return _filterBuilder;
        }

        public FilterBuilder<T> Gt<U>(U? value)
        {
            _op = _opBuilder(new GreaterThen<U>(value));
            _filterBuilder._conditionBuilder = this;
            return _filterBuilder;
        }

        public FilterBuilder<T> In<U>(IEnumerable<U?> value)
        {
            _op = _opBuilder(new In<U>(value));
            _filterBuilder._conditionBuilder = this;
            return _filterBuilder;
        }

        public ConditionBuilder<T> Not()
        {
            var old = _opBuilder;
            _opBuilder = x => new Not(old(x));
            return this;
        }

        internal IParameterOperation CreateQueryProperty()
        {
            return _createQueryProperty(_op!);
        }
    }
}
