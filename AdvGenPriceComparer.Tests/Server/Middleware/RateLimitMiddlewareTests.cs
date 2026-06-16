using System.Threading.Tasks;
using AdvGenPriceComparer.Server.Middleware;
using AdvGenPriceComparer.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvGenPriceComparer.Tests.Server.Middleware
{
    public class RateLimitMiddlewareTests
    {
        [Theory]
        [InlineData("/health")]
        [InlineData("/health/status")]
        [InlineData("/swagger")]
        [InlineData("/swagger/index.html")]
        [InlineData("/")]
        public async Task InvokeAsync_SkipsRateLimitingForExcludedPaths(string path)
        {
            var nextMock = new Mock<RequestDelegate>();
            var loggerMock = new Mock<ILogger<RateLimitMiddleware>>();
            var rateLimitServiceMock = new Mock<IRateLimitService>();

            var middleware = new RateLimitMiddleware(nextMock.Object, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = path;

            await middleware.InvokeAsync(context, rateLimitServiceMock.Object);

            nextMock.Verify(n => n(context), Times.Once);
            rateLimitServiceMock.Verify(r => r.IsAllowed(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("/health-fake")]
        [InlineData("/swagger-fake")]
        public async Task InvokeAsync_AppliesRateLimitingForBypassPaths(string path)
        {
            var nextMock = new Mock<RequestDelegate>();
            var loggerMock = new Mock<ILogger<RateLimitMiddleware>>();
            var rateLimitServiceMock = new Mock<IRateLimitService>();
            rateLimitServiceMock.Setup(r => r.IsAllowed(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            rateLimitServiceMock.Setup(r => r.GetRemainingRequests(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(99);

            var middleware = new RateLimitMiddleware(nextMock.Object, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = path;

            await middleware.InvokeAsync(context, rateLimitServiceMock.Object);

            rateLimitServiceMock.Verify(r => r.IsAllowed(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}
