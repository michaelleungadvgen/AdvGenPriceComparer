using System.Net;
using AdvGenPriceComparer.Server.Middleware;
using AdvGenPriceComparer.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvGenPriceComparer.Tests.Middleware;

public class ApiKeyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SkipsAuthenticationForSwagger()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";
        var nextMock = new Mock<RequestDelegate>();
        var loggerMock = new Mock<ILogger<ApiKeyMiddleware>>();
        var apiKeyServiceMock = new Mock<IApiKeyService>();
        var envMock = new Mock<IWebHostEnvironment>();

        var middleware = new ApiKeyMiddleware(nextMock.Object, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, apiKeyServiceMock.Object, envMock.Object);

        // Assert
        nextMock.Verify(next => next(context), Times.Once);
        Assert.Equal(200, context.Response.StatusCode); // Default OK
    }

    [Fact]
    public async Task InvokeAsync_DoesNotSkipAuthenticationForSwaggerPrefixSpoof()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger-fake-endpoint";
        var nextMock = new Mock<RequestDelegate>();
        var loggerMock = new Mock<ILogger<ApiKeyMiddleware>>();
        var apiKeyServiceMock = new Mock<IApiKeyService>();
        var envMock = new Mock<IWebHostEnvironment>();

        var middleware = new ApiKeyMiddleware(nextMock.Object, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, apiKeyServiceMock.Object, envMock.Object);

        // Assert
        nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }
}
