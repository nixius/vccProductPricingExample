using System.Threading.Tasks;
using VCC.ProductPricingApiTest.Models.DataAccess;

namespace VCC.ProductPricingApiTest.DataAccess
{
    public interface IProductDataAccess
    {
       Task<DbProduct> AddProductAsync(DbProduct product);
       Task<List<DbProduct>> GetProductsAsync();
       Task<List<DbProductDiscount>> GetDiscountsAsync();
       Task<DbProductDiscount> GetDiscountsForProductAsync(int prodictId);
       Task<DbProduct> GetProductByIdAsync(int productId);
       Task<DbProductHistory> GetProductHistoryByIdAsync(int productId);
       Task SetDiscountPriceAsync(int productId, decimal discount);
       Task LogDiscountPriceHistoryAsync(int productId, decimal discountPercm, decimal prevPrice, decimal newPrice);
       Task<bool> UpdateProductAsync(DbProduct dbProd);
       Task<bool> UpdatePriceAsync(int productId, decimal price, decimal? discountPercentage);
    }
}