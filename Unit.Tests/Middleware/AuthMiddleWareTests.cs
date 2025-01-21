using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using CSharpAPI.Middleware;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;


namespace Unit.Tests.Middleware
{
    public class AuthMiddlewareTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthMiddleware _middleware;
        private readonly RequestDelegate _next;

        public AuthMiddlewareTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _next = (HttpContext context) => Task.CompletedTask;
            _middleware = new AuthMiddleware(_next);
        }

        [Fact]
        public async Task InvokeAsync_Returns401_WhenNoApiKey()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns401_WhenInvalidApiKey()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["API_KEY"] = "invalid_key";

            _mockAuthService
                .Setup(x => x.GetUserByApiKey(It.IsAny<string>()))
                .ReturnsAsync((ApiUser)null);

            // Act
            await _middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns403_WhenUserLacksAccess()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["API_KEY"] = "valid_key";
            context.Request.Path = "/api/v1/items";
            context.Request.Method = "GET";

            var user = new ApiUser { api_key = "valid_key", role = "User" };

            _mockAuthService
                .Setup(x => x.GetUserByApiKey("valid_key"))
                .ReturnsAsync(user);

            _mockAuthService
                .Setup(x => x.HasAccess(user, "items", "GET"))
                .ReturnsAsync(false);

            // Act
            await _middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_CallsNext_WhenUserHasAccess()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["API_KEY"] = "valid_key";
            context.Request.Path = "/api/v1/items";
            context.Request.Method = "GET";

            var user = new ApiUser { api_key = "valid_key", role = "Admin" };

            _mockAuthService
                .Setup(x => x.GetUserByApiKey("valid_key"))
                .ReturnsAsync(user);

            _mockAuthService
                .Setup(x => x.HasAccess(user, "items", "GET"))
                .ReturnsAsync(true);

            var nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AuthMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal(user, context.Items["User"]);
        }

        [Fact]
        public async Task InvokeAsync_Returns403_WhenPathIsNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["API_KEY"] = "valid_key";
            context.Request.Path = "/api/v1";  // No resource path

            var user = new ApiUser { api_key = "valid_key", role = "User" };

            _mockAuthService
                .Setup(x => x.GetUserByApiKey("valid_key"))
                .ReturnsAsync(user);

            // Act
            await _middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("/api/v1/items/123", "items")]
        [InlineData("/api/v1/orders/pending", "orders")]
        [InlineData("/api/v1/warehouses/inventory", "warehouses")]
        public async Task InvokeAsync_ExtractsCorrectResourcePath(string requestPath, string expectedResource)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["API_KEY"] = "valid_key";
            context.Request.Path = requestPath;
            context.Request.Method = "GET";

            var user = new ApiUser { api_key = "valid_key", role = "Admin" };

            _mockAuthService
                .Setup(x => x.GetUserByApiKey("valid_key"))
                .ReturnsAsync(user);

            _mockAuthService
                .Setup(x => x.HasAccess(user, expectedResource, "GET"))
                .ReturnsAsync(true)
                .Verifiable();

            // Act
            await _middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            _mockAuthService.Verify();
        }

        [Fact]
        public async Task InvokeAsync_HandlesEmptyPath()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["API_KEY"] = "valid_key";
            context.Request.Path = "";

            var user = new ApiUser { api_key = "valid_key", role = "User" };

            _mockAuthService
                .Setup(x => x.GetUserByApiKey("valid_key"))
                .ReturnsAsync(user);

            // Act
            await _middleware.InvokeAsync(context, _mockAuthService.Object);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
        }
    }
}
