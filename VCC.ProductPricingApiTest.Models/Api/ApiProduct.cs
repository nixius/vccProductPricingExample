using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApiProduct
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("price")]
        public decimal CurrentPrice { get; set; }
        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdatedUtc { get; set; }
    }
}