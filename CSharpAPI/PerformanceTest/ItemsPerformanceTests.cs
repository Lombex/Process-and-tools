using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CSharpAPI.PerformanceTests
{
    public class ItemsPerformanceTests
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5001/api/items";

        public ItemsPerformanceTests()
        {
            _client = new HttpClient();
        }

        // Helper: Mock item toevoegen
        private async Task<string> AddMockItem()
        {
            var mockItem = new
            {
                uid = "ITEM-TEST",
                code = "CODE-123",
                description = "Test Item",
                short_description = "Test Short Description",
                upc_code = "UPC12345",
                model_number = "MOD-123",
                commodity_code = "COM-123",
                item_line = 1,
                item_group = 1,
                item_type = 1,
                unit_purchase_quantity = 10,
                unit_order_quantity = 5,
                pack_order_quantity = 20,
                supplier_id = 1,
                supplier_code = "SUP-123",
                supplier_part_number = "SUP-PART-123",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(mockItem), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<dynamic>(responseData);
            return createdItem?.uid ?? string.Empty;
        }

        [Fact]
        public async Task GetAllItems_PerformanceTest()
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync(BaseUrl);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetAllItems took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task GetItemById_PerformanceTest()
        {
            var itemId = await AddMockItem();

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync($"{BaseUrl}/{itemId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetItemById took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task CreateItem_PerformanceTest()
        {
            var newItem = new
            {
                uid = "ITEM-NEW",
                code = "CODE-NEW",
                description = "New Item",
                short_description = "New Short Description",
                upc_code = "UPCNEW12345",
                model_number = "MOD-NEW",
                commodity_code = "COM-NEW",
                item_line = 2,
                item_group = 2,
                item_type = 2,
                unit_purchase_quantity = 15,
                unit_order_quantity = 7,
                pack_order_quantity = 25,
                supplier_id = 2,
                supplier_code = "SUP-NEW",
                supplier_part_number = "SUP-PART-NEW",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(newItem), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsync(BaseUrl, content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"CreateItem took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task UpdateItem_PerformanceTest()
        {
            var itemId = await AddMockItem();
            var updatedItem = new
            {
                uid = itemId,
                code = "CODE-UPDATED",
                description = "Updated Item",
                short_description = "Updated Short Description",
                upc_code = "UPCUPDATED12345",
                model_number = "MOD-UPDATED",
                commodity_code = "COM-UPDATED",
                item_line = 3,
                item_group = 3,
                item_type = 3,
                unit_purchase_quantity = 20,
                unit_order_quantity = 10,
                pack_order_quantity = 30,
                supplier_id = 3,
                supplier_code = "SUP-UPDATED",
                supplier_part_number = "SUP-PART-UPDATED",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedItem), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PutAsync($"{BaseUrl}/{itemId}", content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"UpdateItem took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task DeleteItem_PerformanceTest()
        {
            var itemId = await AddMockItem();

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.DeleteAsync($"{BaseUrl}/{itemId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"DeleteItem took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }
    }
}
