using System.Reflection;
using CleanApi.Application.Common.Behaviors;
using CleanApi.Application.Modules.Products;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApi.Application;

/// <summary>Composition root for the Application layer: MediatR, pipeline behaviors, validators.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Behaviors run outer→inner in registration order.
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Source-generated Mapperly mappers (stateless).
        services.AddSingleton<ProductMapper>();

        return services;
    }
}
