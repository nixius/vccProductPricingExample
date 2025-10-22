using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class UpdateProductPriceRequest
    {
        [JsonPropertyName("newPrice")]
        public decimal NewPrice { get; set; }
    }
}