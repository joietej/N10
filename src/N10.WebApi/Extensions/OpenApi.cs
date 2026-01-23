using Microsoft.OpenApi;

namespace N10.WebApi.Extensions;

public static class OpenApi
{
    public static IServiceCollection AddOpenApiWithSecurityScheme(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, token) =>
            {
                var components = document.Components ?? new OpenApiComponents();
                document.Components = components;
                components.SecuritySchemes!.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'"
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
