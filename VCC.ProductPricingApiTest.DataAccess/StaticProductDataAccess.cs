using VCC.ProductPricingApiTest.Models.DataAccess;

namespace VCC.ProductPricingApiTest.DataAccess
{
    public class StaticProductDataAccess : IProductDataAccess
    {
        public async Task<DbProduct> AddProductAsync(DbProduct product)
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.AddProduct(product)));
        }

        public async Task<List<DbProduct>> GetProductsAsync()
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.GetProducts()));
        }

        public async Task<DbProduct> GetProductByIdAsync(int productId)
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.GetProduct(productId)));
        }

        public async Task<DbProductHistory> GetProductHistoryByIdAsync(int productId)
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.GetProductPriceHistory(productId)));
        }

        public async Task SetDiscountPriceAsync(int productId, decimal discount)
        {
            await (Task.Run(() => StaticProductDbContext.Instance.SetProductDiscount(productId, discount)));
        }

        public async Task LogDiscountPriceHistoryAsync(int productId, decimal discountPerc, decimal prevPrice, decimal newPrice)
        {
            await (Task.Run(() => StaticProductDbContext.Instance.LogDiscountPriceHistoryAsync(productId, discountPerc, prevPrice, newPrice)));
        }

        public async Task<bool> UpdateProductAsync(DbProduct dbProd)
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.UpdateProduct(dbProd.ProductId, dbProd.Name, dbProd.Price, null)));
        }

        public async Task<bool> UpdatePriceAsync(int productId, decimal price, decimal? discountPercentage)
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.UpdateProduct(productId, null, price, discountPercentage)));
        }

        public async Task<List<DbProductDiscount>> GetDiscountsAsync()
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.GetDiscountsAsync()));
        }

        public async Task<DbProductDiscount> GetDiscountsForProductAsync(int productId)
        {
            return await (Task.Run(() => StaticProductDbContext.Instance.GetDiscountsForProductAsync(productId)));

        }
    }
}