using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApplyProductDiscountResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }
        [JsonPropertyName("discountedPrice")]
        public decimal DiscountedPrice { get; set; }
    }
}