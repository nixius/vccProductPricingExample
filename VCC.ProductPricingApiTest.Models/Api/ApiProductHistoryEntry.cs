using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.Api
{
    public class ApiProductHistoryEntry
    {
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
