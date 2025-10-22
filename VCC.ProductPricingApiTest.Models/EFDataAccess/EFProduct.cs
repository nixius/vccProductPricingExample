namespace VCC.ProductPricingApiTest.Models.EFDataAccess
{
    public class EFProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<EFProductPriceHistory> PriceHistory { get; set; } = new();
        public EFProductDiscount? Discount { get; set; }
    }
}
