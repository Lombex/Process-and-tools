using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CSharpAPI.PerformanceTests
{
    public class WarehousesPerformanceTests
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5001/api/warehouses";

        public WarehousesPerformanceTests()
        {
            _client = new HttpClient();
        }

        private async Task<int> AddMockWarehouse()
        {
            var mockWarehouse = new
            {
                code = "WH-TEST",
                name = "Test Warehouse",
                address = "123 Test Street",
                zip = "12345",
                city = "Test City",
                province = "Test Province",
                country = "Test Country",
                contact = new
                {
                    name = "John Doe",
                    phone = "1234567890",
                    email = "john.doe@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(mockWarehouse), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var createdWarehouse = JsonSerializer.Deserialize<dynamic>(responseData);
            return createdWarehouse?.id ?? 0; 
        }

        [Fact]
        public async Task GetAllWarehouses_PerformanceTest()
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync(BaseUrl);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetAllWarehouses took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task GetWarehouseById_PerformanceTest()
        {
            var warehouseId = await AddMockWarehouse();
            var stopwatch = Stopwatch.StartNew();

            var response = await _client.GetAsync($"{BaseUrl}/{warehouseId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetWarehouseById took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task CreateWarehouse_PerformanceTest()
        {
            var newWarehouse = new
            {
                code = "WH-NEW",
                name = "New Warehouse",
                address = "456 New Street",
                zip = "67890",
                city = "New City",
                province = "New Province",
                country = "New Country",
                contact = new
                {
                    name = "Jane Doe",
                    phone = "9876543210",
                    email = "jane.doe@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(newWarehouse), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsync(BaseUrl, content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"CreateWarehouse took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task UpdateWarehouse_PerformanceTest()
        {
            var warehouseId = await AddMockWarehouse(); 
            var updatedWarehouse = new
            {
                code = "WH-UPDATED",
                name = "Updated Warehouse",
                address = "789 Updated Street",
                zip = "54321",
                city = "Updated City",
                province = "Updated Province",
                country = "Updated Country",
                contact = new
                {
                    name = "Jane Updated",
                    phone = "1231231234",
                    email = "jane.updated@example.com"
                },
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedWarehouse), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PutAsync($"{BaseUrl}/{warehouseId}", content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"UpdateWarehouse took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task DeleteWarehouse_PerformanceTest()
        {
            var warehouseId = await AddMockWarehouse();

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.DeleteAsync($"{BaseUrl}/{warehouseId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"DeleteWarehouse took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }
    }
}
