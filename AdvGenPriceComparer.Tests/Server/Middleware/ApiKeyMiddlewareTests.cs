using System.Threading.Tasks;
using AdvGenPriceComparer.Server.Middleware;
using AdvGenPriceComparer.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvGenPriceComparer.Tests.Server.Middleware
{
    public class ApiKeyMiddlewareTests
    {
        [Theory]
        [InlineData("/health")]
        [InlineData("/health/status")]
        [InlineData("/swagger")]
        [InlineData("/swagger/index.html")]
        [InlineData("/")]
        public async Task InvokeAsync_SkipsValidationForExcludedPaths(string path)
        {
            var nextMock = new Mock<RequestDelegate>();
            var loggerMock = new Mock<ILogger<ApiKeyMiddleware>>();
            var apiKeyServiceMock = new Mock<IApiKeyService>();
            var envMock = new Mock<IWebHostEnvironment>();

            var middleware = new ApiKeyMiddleware(nextMock.Object, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = path;

            await middleware.InvokeAsync(context, apiKeyServiceMock.Object, envMock.Object);

            nextMock.Verify(n => n(context), Times.Once);
        }

        [Theory]
        [InlineData("/health-fake")]
        [InlineData("/swagger-fake")]
        [InlineData("/api/prices-fake")]
        public async Task InvokeAsync_RequiresApiKeyForBypassPaths(string path)
        {
            var nextMock = new Mock<RequestDelegate>();
            var loggerMock = new Mock<ILogger<ApiKeyMiddleware>>();
            var apiKeyServiceMock = new Mock<IApiKeyService>();
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns("Development");

            var middleware = new ApiKeyMiddleware(nextMock.Object, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Method = "GET";

            await middleware.InvokeAsync(context, apiKeyServiceMock.Object, envMock.Object);

            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            nextMock.Verify(n => n(context), Times.Never);
        }
    }
}
