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
            if(context.Metadata.ModelType.IsGenericType)
            {
                var defType = context.Metadata.ModelType.GetGenericTypeDefinition();
                if(defType == typeof(Query<>) || defType == typeof(IQuery<>))
                    return new QueryBinder(_options);
            }

            return null;
        }
    }
}
