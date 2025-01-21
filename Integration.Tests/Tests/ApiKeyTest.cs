using CSharpAPI.Controllers;
using CSharpAPI.Models.Auth;
using CSharpAPI.Services.Auth;
using CSharpAPI.Data;
using FluentAssertions;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Tests.Tests
{
    public class ApiKeyControllerTest : IntegrationTestBase
    {
        private readonly ApiKeysController _controller;
        private readonly IApiKeyService _service;
        private readonly IAuthService _authService;

        public ApiKeyControllerTest()
        {
            _authService = new AuthService(DbContext, Configuration);
            _service = new ApiKeyService(DbContext, _authService);  // Pass both required parameters
            _controller = new ApiKeysController(_service, _authService);

            // Set up admin auth by default
            SetupAdminUserContext(_controller);

            // Clear existing data
            DbContext.ApiUsers.RemoveRange(DbContext.ApiUsers.Where(u => u.role != "Admin"));
            DbContext.SaveChanges();

            // Seed the database with roles, users, and permissions
            DatabaseSeeding.SeedDatabase(DbContext, _authService).Wait();

            // Seed test data
            SeedTestData().Wait();
        }


        private async Task SeedTestData()
        {
            var apiUsers = new List<ApiUser>
            {
                new ApiUser
                {
                    api_key = "test_key_1",
                    app = "Test App 1",
                    role = "Warehouse_Manager",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new ApiUser
                {
                    api_key = "test_key_2",
                    app = "Test App 2",
                    role = "Operative",
                    warehouse_id = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            await DbContext.ApiUsers.AddRangeAsync(apiUsers);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllKeys_ReturnsAllKeys_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAllKeys();
            
            // Assert
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var keys = okResult.Value as IEnumerable<ApiUser>;
            keys.Should().NotBeNull();
            keys.Should().Contain(k => k.api_key == "test_key_1");
            keys.Should().Contain(k => k.api_key == "test_key_2");
        }

        [Fact]
        public async Task GetAllKeys_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAllKeys();

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetKeyById_ReturnsKey_WhenAuthorized()
        {
            // Arrange
            var key = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.api_key == "test_key_1");
            key.Should().NotBeNull();

            // Act
            var actionResult = await _controller.GetKeyById(key.id);
            var result = actionResult.Result as OkObjectResult;
            var returnedKey = result?.Value as ApiUser;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            returnedKey.Should().NotBeNull();
            returnedKey.api_key.Should().Be("test_key_1");
            returnedKey.role.Should().Be("Warehouse_Manager");
        }

        [Fact]
        public async Task GetKeyById_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var key = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.api_key == "test_key_1");
            key.Should().NotBeNull();

            // Act
            var result = await _controller.GetKeyById(key.id);

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateKey_AddsNewKey_WhenAuthorized()
        {
            // Arrange
            var newKey = new ApiUser
            {
                api_key = "test_key_3",
                app = "Test App 3",
                role = "Supervisor",
                warehouse_id = 1,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act
            var actionResult = await _controller.CreateKey(newKey);
            var result = actionResult.Result as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
            var createdKey = result.Value as ApiUser;
            createdKey.Should().NotBeNull();
            createdKey.app.Should().Be("Test App 3");
            createdKey.role.Should().Be("Supervisor");
        }

        [Fact]
        public async Task CreateKey_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var newKey = new ApiUser
            {
                app = "Test App 3",
                role = "Supervisor"
            };

            // Act
            var result = await _controller.CreateKey(newKey);

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GenerateKey_CreatesNewKey_WhenAuthorized()
        {
            // Arrange
            var request = new KeyGenerationRequest
            {
                app = "Test App 3",
                role = "Supervisor",
                warehouse_id = 1
            };

            // Act
            var actionResult = await _controller.GenerateKey(request);
            var result = actionResult.Result as OkObjectResult;
            var generatedKey = result?.Value as ApiUser;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            generatedKey.Should().NotBeNull();
            generatedKey.app.Should().Be("Test App 3");
            generatedKey.role.Should().Be("Supervisor");
            generatedKey.api_key.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateKey_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var request = new KeyGenerationRequest
            {
                app = "Test App 3",
                role = "Supervisor"
            };

            // Act
            var result = await _controller.GenerateKey(request);

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task UpdateKey_UpdatesExistingKey_WhenAuthorized()
        {
            // Arrange
            var key = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.api_key == "test_key_1");
            key.Should().NotBeNull();

            var updateKey = new ApiUser
            {
                app = "Updated App",
                role = "Supervisor",
                warehouse_id = 1
            };

            // Act
            var result = await _controller.UpdateKey(key.id, updateKey);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            var updatedKey = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.id == key.id);
            updatedKey.Should().NotBeNull();
            updatedKey.app.Should().Be("Updated App");
            updatedKey.role.Should().Be("Supervisor");
        }

        [Fact]
        public async Task UpdateKey_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var key = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.api_key == "test_key_1");
            key.Should().NotBeNull();

            var updateKey = new ApiUser
            {
                app = "Updated App",
                role = "Supervisor"
            };

            // Act
            var result = await _controller.UpdateKey(key.id, updateKey);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteKey_RemovesKey_WhenAuthorized()
        {
            // Arrange
            var key = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.api_key == "test_key_1");
            key.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteKey(key.id);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            var deletedKey = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.id == key.id);
            deletedKey.Should().BeNull();
        }

        [Fact]
        public async Task DeleteKey_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");
            var key = await DbContext.ApiUsers.FirstOrDefaultAsync(k => k.api_key == "test_key_1");
            key.Should().NotBeNull();

            // Act
            var result = await _controller.DeleteKey(key.id);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetAvailableRoles_ReturnsRoles_WhenAuthorized()
        {
            // Act
            var actionResult = await _controller.GetAvailableRoles();
            var result = actionResult.Result as OkObjectResult;
            var roles = result?.Value as IEnumerable<string>;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            roles.Should().NotBeNull();
            roles.Should().Contain("Admin");
            roles.Should().Contain("Warehouse_Manager");
            roles.Should().Contain("Operative");
        }

        [Fact]
        public async Task GetAvailableRoles_ReturnsForbidden_WhenUnauthorized()
        {
            // Arrange
            SetupUserContextByRole(_controller, "Operative");

            // Act
            var result = await _controller.GetAvailableRoles();

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }
    }
}
