using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery
{
    public abstract class ComparisonOperator
    {
        protected ComparisonOperator(string[] identifiers)
        {
            Identifiers = identifiers;
        }

        private static ConcurrentDictionary<Type, TypeConverter> _typeConvertCache = new ConcurrentDictionary<Type, TypeConverter>();

        private static object? ConvertValue(string valueString, Type type)
        {
            string? str = valueString.Length == 0 ? null : valueString;
            if(type == typeof(string)) return str;

            var converter = _typeConvertCache.GetOrAdd(type, TypeDescriptor.GetConverter);

            try
            {
                return converter.ConvertFromString(str);
            }
            catch
            {
                throw new NotSupportedException($"Can not convert {str ?? "{null}"} to type {type.Name}.");
            }
        }

        public static ComparisonOperator Eq {get;} = new EqualsOperator();

        public static ComparisonOperator In {get;} = new InOperator();

        public static ComparisonOperator GT {get;} = new GreaterThenOperator();

        public static ComparisonOperator GTE {get;} = new GreaterThanOrEqualOperator();

        public static ComparisonOperator LT {get;} = new LessThanOperator();

        public static ComparisonOperator LTE {get;} = new LessThanOrEqualOperator();

        public static ComparisonOperator NE {get;} = new NotEqualOperator();

        public string[] Identifiers {get;}

        public virtual IEnumerable<string> CombineKeys(string name)
        {
            return Identifiers.Select(i => i.Length > 0 ? $"{name}[{i}]" : name);
        }

        public abstract Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector);

        private class EqualsOperator : ComparisonOperator
        {
            public EqualsOperator() : base(new[] {"", "$eq"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                var value = ConvertValue(stringValues.First(), selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.Equal(selector, constant);
            }
        }
  
        private class InOperator : ComparisonOperator
        {
            private static MethodInfo _containsMethod
                = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2);

            private static MethodInfo CastMethod
                = typeof(Enumerable).GetMethod("Cast")!;

            public InOperator() : base(new[] {"$in"})
            {
            }

            public override IEnumerable<string> CombineKeys(string name)
            {
                return base.CombineKeys(name).Select(it => it + "[]");
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                object value = stringValues.Select(it => ConvertValue(it, selector.Type));
                value = CastMethod.MakeGenericMethod(selector.Type).Invoke(null, new[] {value})!;
                var constant = Expression.Constant(value, typeof(IEnumerable<>).MakeGenericType(selector.Type));
                var contains = _containsMethod.MakeGenericMethod(selector.Type);

                return Expression.Call(contains, constant, selector);
            }
        }

        private class GreaterThenOperator : ComparisonOperator
        {
            public GreaterThenOperator() : base(new[] {"$gt"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                var value = ConvertValue(stringValues.First(), selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.GreaterThan(selector, constant);
            }
        }

        private class GreaterThanOrEqualOperator : ComparisonOperator
        {
            public GreaterThanOrEqualOperator() : base(new[] {"$gte"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                var value = ConvertValue(stringValues.First(), selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.GreaterThanOrEqual(selector, constant);
            }
        }

        private class LessThanOperator : ComparisonOperator
        {
            public LessThanOperator() : base(new[] {"$lt"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                var value = ConvertValue(stringValues.First(), selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.LessThan(selector, constant);
            }
        }

        private class LessThanOrEqualOperator : ComparisonOperator
        {
            public LessThanOrEqualOperator() : base(new[] {"$lte"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                var value = ConvertValue(stringValues.First(), selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.LessThanOrEqual(selector, constant);
            }
        }

        private class NotEqualOperator : ComparisonOperator
        {
            public NotEqualOperator() : base(new[] {"$ne"})
            {
            }

            public override Expression CombineExpression(IEnumerable<string> stringValues, MemberExpression selector)
            {
                var value = ConvertValue(stringValues.First(), selector.Type);
                var constant = Expression.Constant(value, selector.Type);
                return Expression.NotEqual(selector, constant);
            }
        }
    }
}
