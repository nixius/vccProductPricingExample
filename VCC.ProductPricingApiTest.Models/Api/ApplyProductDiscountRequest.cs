using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApplyProductDiscountRequest
    {
        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }
    }
}