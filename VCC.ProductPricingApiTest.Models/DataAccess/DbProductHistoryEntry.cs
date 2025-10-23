namespace VCC.ProductPricingApiTest.Models.DataAccess
{
    public class DbProductHistoryEntry
    {
        public int ProductHistoryEntryId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime Date { get; set; }
    }
}