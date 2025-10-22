using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApiProductHistory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("priceHistory")]
        public List<ApiProductHistoryEntry> PriceHistory { get; set; } = new();
    }
}