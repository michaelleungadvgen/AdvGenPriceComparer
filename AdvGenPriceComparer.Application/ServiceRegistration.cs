using AdvGenPriceComparer.Application.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AdvGenPriceComparer.Application;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Adds Application layer services to the DI container, including custom Mediator and all handlers
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register custom Mediator
        services.AddSingleton<IMediator, Mediator.Mediator>();

        // Register all request handlers from this assembly
        RegisterHandlers(services, Assembly.GetExecutingAssembly());

        return services;
    }

    /// <summary>
    /// Registers all IRequestHandler implementations from the specified assembly
    /// </summary>
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        // Get all types in the assembly
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            // Skip abstract classes, interfaces, and non-public types
            if (!type.IsClass || type.IsAbstract || !type.IsPublic)
                continue;

            // Get all interfaces implemented by this type
            var interfaces = type.GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                // Check if this is an IRequestHandler<,> interface
                if (interfaceType.IsGenericType)
                {
                    var genericDef = interfaceType.GetGenericTypeDefinition();
                    
                    if (genericDef == typeof(IRequestHandler<,>))
                    {
                        // This is a handler with a response
                        services.AddTransient(interfaceType, type);
                    }
                    else if (genericDef == typeof(IRequestHandler<>))
                    {
                        // This is a handler without a response
                        services.AddTransient(interfaceType, type);
                    }
                }
            }
        }
    }
}
