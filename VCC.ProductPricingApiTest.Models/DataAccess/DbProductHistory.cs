using System.Text.Json.Serialization;

namespace VCC.ProductPricingApiTest.Models.DataAccess
{
    public class DbProductHistory
    {
        public int ProductHistoryId { get; set; }
        public string Name { get; set; }
        public List<DbProductHistoryEntry> ProductHistory { get; set; } = new();
    }
}