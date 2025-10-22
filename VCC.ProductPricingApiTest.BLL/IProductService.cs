using VCC.ProductPricingApiTest.Models.Api;

namespace VCC.ProductPricingApiTest.BLL
{
    public interface IProductService
    {
        Task<List<ApiProduct>> GetProductsAsync();
        Task<List<ApiProductDiscount>> GetDiscountsAsync();
        Task<ApiProductDiscount> GetDiscountForProductAsync(int productId);
        Task<ApiProductHistory> GetProductHistoryByIdAsync(int productId);
        Task<ApiProduct> SetDiscountPriceAsync(int productId, decimal discount);
        Task<ApiProduct> UpdatePriceAsync(int productId, decimal discount);
    }
}