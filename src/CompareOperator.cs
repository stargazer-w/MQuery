using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery
{
    public abstract class CompareOperator
    {
        public CompareOperator(string[] identifiers)
        {
            Identifiers = identifiers;
        }

        private static object ConvertSingleValue(string valueString, TypeConverter typeConverter, Type type)
        {
            string? str = valueString.Length == 0 ? null : valueString;
            try
            {
                return typeConverter.ConvertFrom(str);
            }
            catch
            {
                throw new NotSupportedException($"Can not convert {str?? "{null}"} to type {type.Name}.");
            }
        }

        public static CompareOperator Eq { get; set; } = new EqaulsOperatior();
        public static CompareOperator In { get; set; } = new InOperatior();
        public static CompareOperator GT { get; set; } = new GreaterThenOperatior();
        public static CompareOperator GTE { get; set; } = new GreaterThanOrEqualOperatior();
        public static CompareOperator LT { get; set; } = new LessThanOperatior();
        public static CompareOperator LTE { get; set; } = new LessThanOrEqualOperatior();
        public static CompareOperator NE { get; set; } = new NotEqualOperatior();

        public string[] Identifiers { get; }

        public IEnumerable<string> CombineKeys(string name)
        {
            return Identifiers.Select(i => i.Length > 0 ? $"{name}[{i}]" : name);
        }

        public abstract Expression CombineExpression(
            IEnumerable<string> stringValues,
            Expression selector,
            TypeConverter typeConverter
        );

        private class EqaulsOperatior : CompareOperator
        {
            public EqaulsOperatior() : base(new string[] { "", "$eq" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.Equal(selector, constant);
            }
        }

        private class InOperatior : CompareOperator
        {
            private static MethodInfo _containsMethod
                = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2);

            private static MethodInfo CastMethod
                = typeof(Enumerable).GetMethod("Cast")!;

            public InOperatior() : base(new string[] { "$in" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                object value = stringValues.Select(it => ConvertSingleValue(it,typeConverter,selector.Type));
                value = CastMethod.MakeGenericMethod(selector.Type).Invoke(null, new object?[] { value })!;
                var constant = Expression.Constant(value, typeof(IEnumerable<>).MakeGenericType(selector.Type));
                var contains = _containsMethod.MakeGenericMethod(selector.Type);

                return Expression.Call(contains, constant, selector);
            }
        }

        private class GreaterThenOperatior : CompareOperator
        {
            public GreaterThenOperatior() : base(new string[] { "$gt" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.GreaterThan(selector, constant);
            }
        }

        private class GreaterThanOrEqualOperatior : CompareOperator
        {
            public GreaterThanOrEqualOperatior() : base(new string[] { "$gte" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.GreaterThanOrEqual(selector, constant);
            }
        }

        private class LessThanOperatior : CompareOperator
        {
            public LessThanOperatior() : base(new string[] { "$lt" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.LessThan(selector, constant);
            }
        }

        private class LessThanOrEqualOperatior : CompareOperator
        {
            public LessThanOrEqualOperatior() : base(new string[] { "$lte" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.LessThanOrEqual(selector, constant);
            }
        }

        private class NotEqualOperatior : CompareOperator
        {
            public NotEqualOperatior() : base(new string[] { "$ne" })
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, Expression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.NotEqual(selector, constant);
            }
        }
    }
}
