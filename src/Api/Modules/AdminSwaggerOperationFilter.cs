using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Modules;

public sealed class AdminSwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>()
            .Any(attribute => attribute.AuthenticationSchemes == AdminAuthenticationHandler.SchemeName))
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(AdminAuthenticationHandler.SchemeName, context.Document)] = []
            }
        ];
    }
}
