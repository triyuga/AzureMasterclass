using AzureMasterclass.Domain;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SwaggerFilter.Filters
{
    

    public class SwaggerDocumentFilter : IDocumentFilter
    {
        private readonly EnabledFeatures _enabledFeatures;

        public SwaggerDocumentFilter(EnabledFeatures enabledFeatures)
        {
            _enabledFeatures = enabledFeatures;
        }

        
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var disabledRoutes = new List<KeyValuePair<string, OpenApiPathItem>>();
            var disabledSchemas = new List<KeyValuePair<string, OpenApiSchema>>();

            if (!_enabledFeatures.Sql) {
                disabledRoutes.AddRange(swaggerDoc.Paths.Where(x => x.Key.ToLower().Contains("book")));
                disabledSchemas.AddRange(swaggerDoc.Components.Schemas.Where(x => x.Key.ToLower().Contains("book")));

                disabledRoutes.AddRange(swaggerDoc.Paths.Where(x => x.Key.ToLower().Contains("author")));
                disabledSchemas.AddRange(swaggerDoc.Components.Schemas.Where(x => x.Key.ToLower().Contains("author")));
            }

            disabledRoutes.ForEach(x => { 
                swaggerDoc.Paths.Remove(x.Key);
            });

            disabledSchemas.ForEach(x => { 
                swaggerDoc.Components.Schemas.Remove(x.Key);
            });
        }
    }
}