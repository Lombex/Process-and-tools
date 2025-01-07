using Xunit;
using Moq;
using CSharpAPI.Service;
using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpAPI.Tests
{
    public class LocationServiceTests
    {
        private readonly DbContextOptions<SQLiteDatabase> _dbContextOptions;

        public LocationServiceTests()
        {
            // Set up In-Memory database options
            _dbContextOptions = new DbContextOptionsBuilder<SQLiteDatabase>()
                .UseInMemoryDatabase(databaseName: "LocationTestDatabase_" + Guid.NewGuid())  // Unique DB name for each test
                .Options;
        }

        private SQLiteDatabase CreateDbContext()
        {
            return new SQLiteDatabase(_dbContextOptions);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfLocations()
        {
            // Arrange: Create mock data for locations
            var locationList = new List<LocationModel>
            {
                new LocationModel 
                { 
                    id = 1, 
                    warehouse_id = 1, 
                    code = "A.1.0", 
                    name = "Row: A, Rack: 1, Shelf: 0", 
                    created_at = DateTime.Parse("2024-12-16T19:06:24.4217294"), 
                    updated_at = DateTime.Parse("2024-12-16T19:06:24.4217295")
                },
                new LocationModel 
                { 
                    id = 2, 
                    warehouse_id = 1, 
                    code = "A.2.0", 
                    name = "Row: A, Rack: 2, Shelf: 0", 
                    created_at = DateTime.Parse("2024-12-16T19:06:24.4217294"), 
                    updated_at = DateTime.Parse("2024-12-16T19:06:24.4217295")
                }
            };

            using (var context = CreateDbContext())
            {
                context.Location.AddRange(locationList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve all locations
            List<LocationModel> result;
            using (var context = CreateDbContext())
            {
                var service = new LocationService(context);
                result = await service.GetAll();
            }

            // Assert: Verify that the correct number of locations is returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsLocation()
        {
            // Arrange: Add a location to the database
            var location = new LocationModel
            {
                id = 1,
                warehouse_id = 1,
                code = "A.1.0",
                name = "Row: A, Rack: 1, Shelf: 0",
                created_at = DateTime.Parse("2024-12-16T19:06:24.4217294"),
                updated_at = DateTime.Parse("2024-12-16T19:06:24.4217295")
            };

            using (var context = CreateDbContext())
            {
                context.Location.Add(location);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve the location by ID
            LocationModel result;
            using (var context = CreateDbContext())
            {
                var service = new LocationService(context);
                result = await service.GetById(1);
            }

            // Assert: Verify that the location was retrieved correctly
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Row: A, Rack: 1, Shelf: 0", result.name);
        }

        [Fact]
        public async Task Add_ValidLocation_CreatesLocation()
        {
            // Arrange: Create a new location
            var newLocation = new LocationModel
            {
                warehouse_id = 3,
                code = "B.1.0",
                name = "Row: B, Rack: 1, Shelf: 0",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Act: Add the new location to the database
            using (var context = CreateDbContext())
            {
                var service = new LocationService(context);
                await service.Add(newLocation);
            }

            // Assert: Verify that the location is added to the database
            using (var context = CreateDbContext())
            {
                var result = await context.Location.FirstOrDefaultAsync(l => l.code == "B.1.0");
                Assert.NotNull(result);
                Assert.Equal("Row: B, Rack: 1, Shelf: 0", result.name);
            }
        }

        [Fact]
        public async Task Update_ValidLocation_UpdatesLocation()
        {
            // Arrange: Add a location to the database
            var location = new LocationModel
            {
                id = 1,
                warehouse_id = 1,
                code = "A.1.0",
                name = "Row: A, Rack: 1, Shelf: 0",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.Location.Add(location);
                await context.SaveChangesAsync();
            }

            // Act: Update the location
            var updatedLocation = new LocationModel
            {
                warehouse_id = 1,
                code = "A.1.0",
                name = "Updated Row: A, Rack: 1, Shelf: 0",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                var service = new LocationService(context);
                var result = await service.Update(1, updatedLocation);
            }

            // Assert: Verify that the location was updated correctly
            using (var context = CreateDbContext())
            {
                var result = await context.Location.FirstOrDefaultAsync(l => l.id == 1);
                Assert.NotNull(result);
                Assert.Equal("Updated Row: A, Rack: 1, Shelf: 0", result.name);
            }
        }

        [Fact]
        public async Task Delete_ValidLocation_DeletesLocation()
        {
            // Arrange: Add a location to the database
            var location = new LocationModel
            {
                id = 1,
                warehouse_id = 1,
                code = "A.1.0",
                name = "Row: A, Rack: 1, Shelf: 0",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            using (var context = CreateDbContext())
            {
                context.Location.Add(location);
                await context.SaveChangesAsync();
            }

            // Act: Delete the location
            using (var context = CreateDbContext())
            {
                var service = new LocationService(context);
                var result = await service.Delete(1);
            }

            // Assert: Verify that the location was deleted
            using (var context = CreateDbContext())
            {
                var result = await context.Location.FirstOrDefaultAsync(l => l.id == 1);
                Assert.Null(result); // The location should be deleted
            }
        }

        [Fact]
        public async Task GetByWarehouseId_ValidWarehouseId_ReturnsLocations()
        {
            // Arrange: Add locations for multiple warehouses
            var locationList = new List<LocationModel>
            {
                new LocationModel
                {
                    id = 1,
                    warehouse_id = 1,
                    code = "A.1.0",
                    name = "Row: A, Rack: 1, Shelf: 0",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new LocationModel
                {
                    id = 2,
                    warehouse_id = 1,
                    code = "A.2.0",
                    name = "Row: A, Rack: 2, Shelf: 0",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                },
                new LocationModel
                {
                    id = 3,
                    warehouse_id = 2,
                    code = "B.1.0",
                    name = "Row: B, Rack: 1, Shelf: 0",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };

            using (var context = CreateDbContext())
            {
                context.Location.AddRange(locationList);
                await context.SaveChangesAsync();
            }

            // Act: Retrieve locations for warehouse_id = 1
            List<LocationModel> result;
            using (var context = CreateDbContext())
            {
                var service = new LocationService(context);
                result = await service.GetByWarehouseId(1);
            }

            // Assert: Verify that only the locations for warehouse_id = 1 are returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
