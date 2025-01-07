using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CSharpAPI.PerformanceTests
{
    public class LocationsPerformanceTests
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5001/api/locations";

        public LocationsPerformanceTests()
        {
            _client = new HttpClient();
        }

        // Helper: Mock locatie toevoegen
        private async Task<int> AddMockLocation()
        {
            var mockLocation = new
            {
                warehouse_id = 1,
                code = "LOC-TEST",
                name = "Test Location",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(mockLocation), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var createdLocation = JsonSerializer.Deserialize<dynamic>(responseData);
            return createdLocation?.id ?? 0;
        }

        [Fact]
        public async Task GetAllLocations_PerformanceTest()
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync(BaseUrl);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetAllLocations took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task GetLocationById_PerformanceTest()
        {
            var locationId = await AddMockLocation();

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync($"{BaseUrl}/{locationId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetLocationById took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task CreateLocation_PerformanceTest()
        {
            var newLocation = new
            {
                warehouse_id = 1,
                code = "LOC-NEW",
                name = "New Location",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(newLocation), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsync(BaseUrl, content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"CreateLocation took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task UpdateLocation_PerformanceTest()
        {
            var locationId = await AddMockLocation();
            var updatedLocation = new
            {
                warehouse_id = 1,
                code = "LOC-UPDATED",
                name = "Updated Location",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedLocation), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PutAsync($"{BaseUrl}/{locationId}", content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"UpdateLocation took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task DeleteLocation_PerformanceTest()
        {
            var locationId = await AddMockLocation();

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.DeleteAsync($"{BaseUrl}/{locationId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"DeleteLocation took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }
    }
}
