using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CSharpAPI.PerformanceTests
{
    public class OrdersPerformanceTests
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5001/api/orders";

        public OrdersPerformanceTests()
        {
            _client = new HttpClient();
        }

        private async Task<int> AddMockOrder()
        {
            var mockOrder = new
            {
                source_id = 1,
                order_date = "2023-12-31",
                request_date = "2024-01-05",
                reference = "TestReference",
                reference_extra = "ExtraReference",
                order_status = "Pending",
                notes = "Test Notes",
                shipping_notes = "Test Shipping Notes",
                picking_notes = "Test Picking Notes",
                warehouse_id = 1,
                ship_to = 1,
                bill_to = 1,
                shipment_id = 1,
                total_amount = 100.50f,
                total_discount = 5.0f,
                total_tax = 10.0f,
                total_surcharge = 2.5f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new[]
                {
                    new { id = 1, code = "Item1", description = "Test Item", quantity = 10 }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(mockOrder), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var createdOrder = JsonSerializer.Deserialize<dynamic>(responseData);
            return createdOrder?.id ?? 0; 
        }

        [Fact]
        public async Task GetAllOrders_PerformanceTest()
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync(BaseUrl);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetAllOrders took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task GetOrderById_PerformanceTest()
        {
            var orderId = await AddMockOrder(); 
            var stopwatch = Stopwatch.StartNew();

            var response = await _client.GetAsync($"{BaseUrl}/{orderId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"GetOrderById took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task CreateOrder_PerformanceTest()
        {
            var newOrder = new
            {
                source_id = 1,
                order_date = "2023-12-31",
                request_date = "2024-01-05",
                reference = "TestReference",
                reference_extra = "ExtraReference",
                order_status = "Pending",
                notes = "Test Notes",
                shipping_notes = "Test Shipping Notes",
                picking_notes = "Test Picking Notes",
                warehouse_id = 1,
                ship_to = 1,
                bill_to = 1,
                shipment_id = 1,
                total_amount = 100.50f,
                total_discount = 5.0f,
                total_tax = 10.0f,
                total_surcharge = 2.5f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new[]
                {
                    new { id = 1, code = "Item1", description = "Test Item", quantity = 10 }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(newOrder), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsync(BaseUrl, content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"CreateOrder took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task UpdateOrder_PerformanceTest()
        {
            var orderId = await AddMockOrder();
            var updatedOrder = new
            {
                source_id = 1,
                order_date = "2023-12-31",
                request_date = "2024-01-05",
                reference = "UpdatedReference",
                reference_extra = "UpdatedExtraReference",
                order_status = "Completed",
                notes = "Updated Notes",
                shipping_notes = "Updated Shipping Notes",
                picking_notes = "Updated Picking Notes",
                warehouse_id = 2,
                ship_to = 2,
                bill_to = 2,
                shipment_id = 2,
                total_amount = 200.50f,
                total_discount = 10.0f,
                total_tax = 20.0f,
                total_surcharge = 5.0f,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                items = new[]
                {
                    new { id = 1, code = "UpdatedItem1", description = "Updated Test Item", quantity = 5 }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedOrder), Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PutAsync($"{BaseUrl}/{orderId}", content);
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"UpdateOrder took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }

        [Fact]
        public async Task DeleteOrder_PerformanceTest()
        {
            var orderId = await AddMockOrder(); 

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.DeleteAsync($"{BaseUrl}/{orderId}");
            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            Assert.True(stopwatch.ElapsedMilliseconds <= 500,
                $"DeleteOrder took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 500 ms limit.");
        }
    }
}
