using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.Application.Mediator;

/// <summary>
/// Default implementation of the mediator pattern
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new instance of the Mediator
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve handlers</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Sends a request to the appropriate handler and returns the response
    /// </summary>
    /// <typeparam name="TResponse">The type of response</typeparam>
    /// <param name="request">The request to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the handler</returns>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Get the handler type
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Resolve the handler
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");

        // Call the handler
        var method = handlerType.GetMethod("Handle");
        if (method == null)
            throw new InvalidOperationException($"Handler for {requestType.Name} does not have a Handle method");

        var result = method.Invoke(handler, new object[] { request, cancellationToken });
        return (Task<TResponse>)result!;
    }

    /// <summary>
    /// Sends a request to the appropriate handler (no response expected)
    /// </summary>
    /// <param name="request">The request to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Get the handler type
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        // Resolve the handler
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");

        // Call the handler
        var method = handlerType.GetMethod("Handle");
        if (method == null)
            throw new InvalidOperationException($"Handler for {requestType.Name} does not have a Handle method");

        var result = method.Invoke(handler, new object[] { request, cancellationToken });
        return (Task)result!;
    }
}
