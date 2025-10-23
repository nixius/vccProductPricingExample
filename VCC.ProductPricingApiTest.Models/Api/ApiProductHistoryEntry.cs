using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApiProductHistoryEntry
    {
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("discountPercentage")]
        public decimal? DiscountPercentage { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
