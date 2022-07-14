using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MQuery.QueryString;

namespace MQuery.AspNetCore
{
    public class QueryBinderProvider : IModelBinderProvider
    {
        private readonly ParserOptions _options;

        public QueryBinderProvider(ParserOptions options)
        {
            _options = options;
        }

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if(!IsDefineOfQuery(context.Metadata.ModelType))
                return null;

            var modelType = context.Metadata.ModelType.GetGenericArguments()[0];
            var binderType = typeof(QueryBinder<>).MakeGenericType(modelType);
            return Activator.CreateInstance(binderType, _options) as IModelBinder;
        }

        public bool IsDefineOfQuery(Type type)
        {
            return type.IsGenericType
                && (
                    type.GetGenericTypeDefinition() == typeof(Query<>)
                    || type.GetGenericTypeDefinition() == typeof(IQuery<>)
                );
        }
    }
}
