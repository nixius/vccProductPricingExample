namespace VCC.ProductPricingApiTest.Models.EFDataAccess
{
    public class EFProductPriceHistory
    {
        public int ProductPriceHistoryId { get; set; }
        public int ProductId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public EFProduct Product { get; set; } = null!;
    }
}
