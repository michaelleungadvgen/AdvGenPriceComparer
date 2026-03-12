namespace AdvGenPriceComparer.Application.Mediator;

/// <summary>
/// Marker interface for a request that returns a response
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequest<TResponse> { }

/// <summary>
/// Marker interface for a request that does not return a response
/// </summary>
public interface IRequest { }

/// <summary>
/// Handles a request that returns a response
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a request that does not return a response
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
public interface IRequestHandler<TRequest> where TRequest : IRequest
{
    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Mediator interface for sending requests to their handlers
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to the appropriate handler and returns the response
    /// </summary>
    /// <typeparam name="TResponse">The type of response</typeparam>
    /// <param name="request">The request to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the handler</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request to the appropriate handler (no response expected)
    /// </summary>
    /// <param name="request">The request to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
