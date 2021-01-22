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

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if(context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Query<>))
                return new QueryBinder(_options);

            return null;
        }
    }
}
