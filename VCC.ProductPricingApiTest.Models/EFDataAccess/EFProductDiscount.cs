namespace VCC.ProductPricingApiTest.Models.EFDataAccess
{
    public class EFProductDiscount
    {
        public int ProductDiscountId { get; set; }
        public int ProductId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public EFProduct Product { get; set; }
    }
}
