using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.DataAccess
{
    public class DbProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdatedUtc {  get; set; }
    }
}