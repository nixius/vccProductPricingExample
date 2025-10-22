using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class UpdateProductPriceResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }
        [JsonPropertyName("newPrice")]
        public decimal? NewPrice { get; set; }
        [JsonPropertyName("lastUpdated")]
        public DateTime? LastUpdatedUtc { get; set; }
    }
}