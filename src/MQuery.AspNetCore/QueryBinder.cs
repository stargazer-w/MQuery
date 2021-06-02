using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using MQuery.QueryString;

namespace MQuery.AspNetCore
{
    public class QueryBinder : IModelBinder
    {
        private readonly ParserOptions _options;

        public QueryBinder(ParserOptions options)
        {
            _options = options;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var queryType = bindingContext.ModelMetadata.ModelType;
            var elementType = queryType.GenericTypeArguments.First();
            var queryString = bindingContext.HttpContext.Request.QueryString;

            var parserType = typeof(QueryParser<>).MakeGenericType(elementType);
            if(
                bindingContext.ModelMetadata is DefaultModelMetadata meta
                && meta.Attributes.ParameterAttributes.FirstOrDefault(a => a is BindAttribute) is BindAttribute bind
            )
            {
                _options.IncludeProps ??= new();
                _options.IncludeProps.AddRange(bind.Include);
            }
            var parser = Activator.CreateInstance(parserType, _options);

            try
            {
                var query = parserType.GetMethod("Parse").Invoke(parser, new object[] { queryString.ToString() });
                bindingContext.Result = ModelBindingResult.Success(query);
                bindingContext.ValidationState.Add(query, new() { SuppressValidation = true });
            }
            catch(TargetInvocationException e) when(e.InnerException is ParseException pe)
            {
                bindingContext.ModelState.AddModelError(pe.Key, pe.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
