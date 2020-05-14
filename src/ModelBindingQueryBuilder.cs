using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Primitives;

namespace MQuery
{
    public class ModelBindingQueryBuilder
    {
        private readonly ModelBindingContext _bindingContext;
        private readonly Query _queryModel;
        private readonly IEnumerable<Attribute> _attributes = Array.Empty<Attribute>();

        private readonly PropertyInfo[] _targetPropertyInfos;

        public ModelBindingQueryBuilder(ModelBindingContext bindingContext)
        {
            _bindingContext = bindingContext;
            var type = bindingContext.ModelType;
            _queryModel = (Query)Activator.CreateInstance(type)!;

            var targetProperties = type.GetGenericArguments()[0].GetProperties();

            if(_bindingContext.ModelMetadata is DefaultModelMetadata md)
            {
                _attributes = md.Attributes.ParameterAttributes.Cast<Attribute>();
            }

            if(_attributes.FirstOrDefault(a => a is BindAttribute) is BindAttribute bind)
            {
                _targetPropertyInfos = targetProperties.Where(p => bind.Include.Contains(p.Name)).ToArray();
            }
            else
            {
                _targetPropertyInfos = targetProperties;
            }
        }

        public Query Build()
        {
            BindWhereExpression();
            BindOrderExpressions();
            BindPaging();

            return _queryModel;
        }

        private void BindPaging()
        {
            var query = _bindingContext.HttpContext.Request.Query;
            if(query.TryGetValue("$skip", out var skipString))
            {
                try
                {
                    _queryModel.Slicing.Skip = int.Parse(skipString.First());
                }
                catch(Exception e)
                {
                    _bindingContext.ModelState.AddModelError("$skip", e.Message);
                }
            }

            if(query.TryGetValue("$limit", out var limitString))
            {
                try
                {
                    _queryModel.Slicing.Limit = int.Parse(limitString.First());
                }
                catch(Exception e)
                {
                    _bindingContext.ModelState.AddModelError("$limit", e.Message);
                }
            }
        }

        private void BindOrderExpressions()
        {
            _queryModel.OrderExpressions = GetSortOpers().ToArray();
        }

        private IEnumerable<(QueryOrderType orderType, LambdaExpression selector)> GetSortOpers()
        {
            var regex = new Regex(@"\$sort\[(.+)\]");
            var query = _bindingContext.HttpContext.Request.Query;
            foreach(var (key, value) in query)
            {
                if(!(regex.Match(key) is { Success: true, Groups: { Count: 2 } } match))
                    continue;


                if(!(_targetPropertyInfos.FirstOrDefault(p =>
                            p.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase)) is { } propInfo
                    )) continue;

                Enum.TryParse<QueryOrderType>(value.First(), out var orderType);
                if(!Enum.IsDefined(typeof(QueryOrderType), orderType))
                {
                    _bindingContext.ModelState.AddModelError(key, "排序值不合法，仅支持1或-1");
                    continue;
                }

                var selectorExpression = Expression.Property(_queryModel.ParameterExpression, propInfo);
                var selector = Expression.Lambda(selectorExpression, _queryModel.ParameterExpression);
                yield return (orderType, selector);
            }
        }

        private void BindWhereExpression()
        {
            var opers = _targetPropertyInfos
                        .SelectMany(BindCompareOperationsOnProperty)
                        .ToArray();

            if(opers.Any())
                _queryModel.WhereExpression =
                    Expression.Lambda(opers.Aggregate(Expression.And), _queryModel.ParameterExpression);
        }

        private IEnumerable<Expression> BindCompareOperationsOnProperty(PropertyInfo propertyInfo)
        {
            var name = propertyInfo.Name;
            var propertySelector = Expression.Property(_queryModel.ParameterExpression, propertyInfo);
            // 相等绑定，与其他操作互斥，优先级最高
            if(TryGetOperatorExpression(ComparisonOperator.Eq, out var eqOper))
            {
                if(eqOper != null) yield return eqOper;
                yield break;
            }

            // 枚举绑定，与其他操作互斥，优先级其次
            if(TryGetOperatorExpression(ComparisonOperator.In, out var inOper))
            {
                if(inOper != null) yield return inOper;
                yield break;
            }

            // 大于绑定，与大于等于互斥，优先级更高
            if(TryGetOperatorExpression(ComparisonOperator.GT, out var gtOper))
            {
                if(gtOper != null) yield return gtOper;
            }
            // 大于等于绑定，与大于互斥
            else if(TryGetOperatorExpression(ComparisonOperator.GTE, out var gteOper))
            {
                if(gteOper != null) yield return gteOper;
            }

            // 小于绑定，与小于等于互斥，优先级更高
            if(TryGetOperatorExpression(ComparisonOperator.LT, out var ltOper))
            {
                if(ltOper != null) yield return ltOper;
            }
            // 小于等于绑定，与小于互斥
            else if(TryGetOperatorExpression(ComparisonOperator.LTE, out var lteOper))
            {
                if(lteOper != null) yield return lteOper;
            }

            // 不等于绑定
            if(TryGetOperatorExpression(ComparisonOperator.NE, out var neOper))
            {
                if(neOper != null) yield return neOper;
            }

            bool TryGetOperatorExpression(ComparisonOperator @operator, out Expression? valueExpression)
            {
                var query = _bindingContext.HttpContext.Request.Query;
                // 驼峰化
                name = char.ToLower(name[0]) + name[1..];

                // 从query中获取查询，若没有值则表示忽略
                var result = @operator.CombineKeys(name)
                                      .Select(key =>
                                      {
                                          var use = query.TryGetValue(key, out var stringValues);
                                          return (use, key, stringValues);
                                      })
                                      .FirstOrDefault(it => it.use);
                if(result == default)
                {
                    valueExpression = null;
                    return false;
                }

                try
                {
                    valueExpression = @operator.CombineExpression(result.stringValues, propertySelector);
                    return true;
                }
                catch(Exception e)
                {
                    _bindingContext.ModelState.AddModelError(result.key, e.Message);
                    valueExpression = null;
                    return true;
                }
            }
        }
    }
}
