using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AdvGenPriceComparer.Application;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Adds Application layer services to the DI container, including MediatR and all handlers
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR - this will scan the current assembly for all handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
