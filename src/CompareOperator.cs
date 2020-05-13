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

        private static object? ConvertSingleValue(string valueString, TypeConverter typeConverter, Type type)
        {
            string? str = valueString.Length == 0 ? null : valueString;
            if(type == typeof(string)) return str;
            try
            {
                return typeConverter.ConvertFrom(str);
            }
            catch
            {
                throw new NotSupportedException($"Can not convert {str ?? "{null}"} to type {type.Name}.");
            }
        }

        public static CompareOperator Eq {get;} = new EqualsOperator();

        public static CompareOperator In {get;} = new InOperator();

        public static CompareOperator GT {get;} = new GreaterThenOperator();

        public static CompareOperator GTE {get;} = new GreaterThanOrEqualOperator();

        public static CompareOperator LT {get;} = new LessThanOperator();

        public static CompareOperator LTE {get;} = new LessThanOrEqualOperator();

        public static CompareOperator NE {get;} = new NotEqualOperator();

        public string[] Identifiers {get;}

        public IEnumerable<string> CombineKeys(string name)
        {
            return Identifiers.Select(i => i.Length > 0 ? $"{name}[{i}]" : name);
        }

        public abstract Expression CombineExpression(
            IEnumerable<string> stringValues,
            MemberExpression selector,
            TypeConverter typeConverter
        );

        private class EqualsOperator : CompareOperator
        {
            public EqualsOperator() : base(new[] {"", "$eq"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.Equal(selector, constant);
            }
        }

        private class InOperator : CompareOperator
        {
            private static MethodInfo _containsMethod
                = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2);

            private static MethodInfo CastMethod
                = typeof(Enumerable).GetMethod("Cast")!;

            public InOperator() : base(new[] {"$in"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                object value = stringValues.Select(it => ConvertSingleValue(it, typeConverter, selector.Type));
                value = CastMethod.MakeGenericMethod(selector.Type).Invoke(null, new[] {value})!;
                var constant = Expression.Constant(value, typeof(IEnumerable<>).MakeGenericType(selector.Type));
                var contains = _containsMethod.MakeGenericMethod(selector.Type);

                return Expression.Call(contains, constant, selector);
            }
        }

        private class GreaterThenOperator : CompareOperator
        {
            public GreaterThenOperator() : base(new[] {"$gt"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.GreaterThan(selector, constant);
            }
        }

        private class GreaterThanOrEqualOperator : CompareOperator
        {
            public GreaterThanOrEqualOperator() : base(new[] {"$gte"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.GreaterThanOrEqual(selector, constant);
            }
        }

        private class LessThanOperator : CompareOperator
        {
            public LessThanOperator() : base(new[] {"$lt"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.LessThan(selector, constant);
            }
        }

        private class LessThanOrEqualOperator : CompareOperator
        {
            public LessThanOrEqualOperator() : base(new[] {"$lte"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.LessThanOrEqual(selector, constant);
            }
        }

        private class NotEqualOperator : CompareOperator
        {
            public NotEqualOperator() : base(new[] {"$ne"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector, TypeConverter typeConverter)
            {
                var value = ConvertSingleValue(stringValues.First(), typeConverter, selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.NotEqual(selector, constant);
            }
        }
    }
}
