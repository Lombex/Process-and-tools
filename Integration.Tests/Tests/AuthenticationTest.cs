using CSharpAPI.Services.Auth;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Integration.Tests
{
    public class AuthenticationTests : IntegrationTestBase
    {
        private readonly AuthService _authService;

        public AuthenticationTests()
        {
            _authService = new AuthService(DbContext);
        }

        [Fact]
        public async Task AdminUser_HasFullAccess()
        {
            // Arrange
            var user = await _authService.GetUserByApiKey(AdminApiKey);

            // Act
            var canView = await _authService.HasAccess(user, "warehouses", "GET");
            var canCreate = await _authService.HasAccess(user, "warehouses", "POST");
            var canUpdate = await _authService.HasAccess(user, "warehouses", "PUT");
            var canDelete = await _authService.HasAccess(user, "warehouses", "DELETE");

            // Assert
            Assert.True(canView);
            Assert.True(canCreate);
            Assert.True(canUpdate);
            Assert.True(canDelete);
        }

        [Fact]
        public async Task ViewerUser_HasLimitedAccess()
        {
            // Arrange
            var user = await _authService.GetUserByApiKey(ViewerApiKey);

            // Act
            var canView = await _authService.HasAccess(user, "warehouses", "GET");
            var canCreate = await _authService.HasAccess(user, "warehouses", "POST");
            var canUpdate = await _authService.HasAccess(user, "warehouses", "PUT");
            var canDelete = await _authService.HasAccess(user, "warehouses", "DELETE");

            // Assert
            Assert.True(canView);
            Assert.False(canCreate);
            Assert.False(canUpdate);
            Assert.False(canDelete);
        }

        [Fact]
        public async Task InvalidApiKey_ReturnsNull()
        {
            // Act
            var user = await _authService.GetUserByApiKey("invalid_key");

            // Assert
            Assert.Null(user);
        }
    }
}