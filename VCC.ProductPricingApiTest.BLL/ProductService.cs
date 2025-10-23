using System.Transactions;
using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.Models.Api;
using VCC.ProductPricingApiTest.Models.DataAccess;

namespace VCC.ProductPricingApiTest.BLL
{
    public class ProductService : IProductService
    {
        private readonly IProductDataAccess _productDB;
        public ProductService(IProductDataAccess productDB)
        {
            _productDB = productDB;
        }

        public async Task<List<ApiProduct>> GetProductsAsync()
        {
            var dbProds = await _productDB.GetProductsAsync();
            var apiProds = new List<ApiProduct>();
            dbProds.ForEach(x => apiProds.Add(ConvertToApiProd(x)));
            return apiProds;
        }

        public async Task<List<ApiProductDiscount>> GetDiscountsAsync()
        {
            var dbDiscs = await _productDB.GetDiscountsAsync();
            var apiProds = new List<ApiProductDiscount>();

            dbDiscs.ForEach(dd => apiProds.Add(new ApiProductDiscount() { Id = dd.ProductId, DiscountPercentage = dd.DiscountPercentage }));

            return apiProds;
        }

        public async Task<ApiProductDiscount> GetDiscountForProductAsync(int productId)
        {
            var dbDisc = await _productDB.GetDiscountsForProductAsync(productId);

            if (dbDisc == null)
                return null;

            return new ApiProductDiscount() { Id = dbDisc.ProductId, DiscountPercentage = dbDisc.DiscountPercentage };
        }


        public async Task<ApiProductHistory> GetProductHistoryByIdAsync(int productId)
        {
            var histProd = await _productDB.GetProductHistoryByIdAsync(productId);
            var retProd = ConvertToApiProdHistory(histProd);
            return retProd;
        }

        public async Task<ApiProduct> SetDiscountPriceAsync(int productId, decimal discount)
        {
            if (discount < 0.0m || discount >= 100.0m)
                return null;

            await _productDB.SetDiscountPriceAsync(productId, discount);
            var updatedProd = await _productDB.GetProductByIdAsync(productId);
            var retprod =  ConvertToApiProd(updatedProd);
            return retprod;
        }

        public async Task<ApiProduct> UpdatePriceAsync(int productId, decimal newPrice)
        {
            if (productId <= 0 || newPrice <= 0.0m)
                return null;

            var result = await _productDB.UpdatePriceAsync(productId, newPrice);
            if (!result)
                return null;

            var latestProd = await _productDB.GetProductByIdAsync(productId);
            var retProd = ConvertToApiProd(latestProd);
            return retProd;
        }

        private ApiProduct ConvertToApiProd(DbProduct dbProd)
        {
            if (dbProd == null)
                return null;

            var apiProd = new ApiProduct()
            {
                Id = dbProd.ProductId,
                Name = dbProd.Name,
                OriginalPrice = dbProd.Price,
                LastUpdatedUtc = dbProd.LastUpdatedUtc
            };

            return apiProd;
        }

        private ApiProductHistory ConvertToApiProdHistory(DbProductHistory dbProd)
        {
            if (dbProd == null)
                return null;

            var apiProd = new ApiProductHistory()
            {
                Id = dbProd.ProductHistoryId,
                Name = dbProd.Name
            };

            dbProd.ProductHistory.ForEach(ph => apiProd.PriceHistory.Add(new ApiProductHistoryEntry() 
            { 
                Date = ph.Date, 
                Price = ph.Price,
                AtDiscount = ph.DiscountPercentage.HasValue
            }));

            return apiProd;
        }

    }
}