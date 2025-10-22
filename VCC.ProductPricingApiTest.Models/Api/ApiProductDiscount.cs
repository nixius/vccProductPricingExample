using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApiProductDiscount
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }
    }
}