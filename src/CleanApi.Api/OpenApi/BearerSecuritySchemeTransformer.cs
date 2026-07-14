using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CleanApi.Api.OpenApi;

/// <summary>
/// Adds a JWT Bearer security scheme to the generated OpenAPI document so the docs UI (Scalar)
/// shows an "Authorize" affordance and sends the token on try-it-out requests.
/// (Microsoft.OpenApi 2.x API: types live in the Microsoft.OpenApi namespace.)
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token (without the 'Bearer' prefix).",
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = scheme;

        var requirement = new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() },
        };

        document.Security ??= [];
        document.Security.Add(requirement);

        return Task.CompletedTask;
    }
}
