using Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Core.Swagger
{
    public class HeaderOptionFilter(IOptions<CoreSettings> options) : IOperationFilter
    {
        private readonly CoreSettings _coreSettings = options.Value;

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = _coreSettings.Localization.HeaderName,
                In = ParameterLocation.Header,
                Required = false,
                Description = $"Culture: {string.Join(" | ", _coreSettings.Localization.SupportedCultures)}",
                Schema = new OpenApiSchema
                {
                    Type = "String",
                    Default = new OpenApiString(_coreSettings.Localization.DefaultCulture)
                }
            });
        }
    }
}