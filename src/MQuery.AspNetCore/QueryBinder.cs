using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using MQuery.QueryString;

namespace MQuery.AspNetCore
{
    public class QueryBinder<T> : IModelBinder
    {
        private readonly ParserOptions _options;

        public QueryBinder(ParserOptions options)
        {
            _options = options;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if(
                bindingContext.ModelMetadata is DefaultModelMetadata meta
                && meta.Attributes.ParameterAttributes!.FirstOrDefault(a => a is BindAttribute) is BindAttribute bind
            )
            {
                _options.IncludeProps ??= new();
                _options.IncludeProps.AddRange(bind.Include);
            }
            var parser = new QueryParser<T>(_options);

            try
            {
                var query = parser.Parse(bindingContext.HttpContext.Request.QueryString.ToString());
                bindingContext.Result = ModelBindingResult.Success(query);
                bindingContext.ValidationState.Add(query, new() { SuppressValidation = true });
            }
            catch(ParseException pe)
            {
                bindingContext.ModelState.AddModelError(pe.Key ?? string.Empty, pe.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
