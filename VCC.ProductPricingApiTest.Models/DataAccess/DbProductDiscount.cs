using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.DataAccess
{
    public class DbProductDiscount
    {
        public int ProductDiscountId { get; set; }
        public int ProductId { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}