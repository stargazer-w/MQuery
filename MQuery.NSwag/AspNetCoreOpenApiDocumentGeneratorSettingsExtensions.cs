using System;
using NSwag.Generation.AspNetCore;

namespace MQuery.NSwag
{
    public static class AspNetCoreOpenApiDocumentGeneratorSettingsExtensions
    {
        public static void AddMQuery(this AspNetCoreOpenApiDocumentGeneratorSettings settings)
        {
            if(settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var processor = new MQueryOpenApiDocumentProcessor();
            settings.OperationProcessors.Add(processor);
            settings.PostProcess = processor.OpenApiDocumentPostProcess;
        }
    }
}
