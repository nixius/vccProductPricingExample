using Microsoft.EntityFrameworkCore;
using VCC.ProductPricingApiTest.Models.DataAccess;
using VCC.ProductPricingApiTest.Models.EFDataAccess;

namespace VCC.ProductPricingApiTest.DataAccess
{
    public class EFProductDataAccess : IProductDataAccess
    {
        private readonly EFProductDbContext _db;

        public EFProductDataAccess(EFProductDbContext db)
        {
            _db = db;
        }

        public async Task<DbProduct> AddProductAsync(DbProduct product)
        {
            // must have viable data
            if (product == null || string.IsNullOrWhiteSpace(product.Name) || product.Price <= 0.0m)
                return null;

            var entity = new EFProduct
            {
                Name = product.Name,
                LastUpdated = DateTime.UtcNow
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync();

            // Also set the price in the history table
            _db.ProductPriceHistories.Add(new EFProductPriceHistory
            {
                ProductId = entity.ProductId,
                Timestamp = DateTime.UtcNow,
                OldPrice = null,    // first price so no old price to have
                NewPrice = product.Price
            });
            entity.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new DbProduct
            {
                ProductId = entity.ProductId,
                Name = entity.Name,
                LastUpdatedUtc = entity.LastUpdated,
                Price = product.Price
            };
        }

        public async Task<List<DbProduct>> GetProductsAsync()
        {
            return await _db.Products
                .Select(p => new DbProduct
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    LastUpdatedUtc = p.LastUpdated,
                    Price = p.Price
                })
                .ToListAsync();
        }

        public async Task<DbProduct> GetProductByIdAsync(int productId)
        {
            var result = await _db.Products
                .Where(p => p.ProductId == productId)
                .Select(p => new DbProduct
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    LastUpdatedUtc = p.LastUpdated,
                    Price = p.Price
                })
                .SingleOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException($"Product {productId} not found.");

            return result;
        }

        public async Task<DbProductHistory> GetProductHistoryByIdAsync(int productId)
        {
            var p = await _db.Products
                .Include(x => x.PriceHistory)
                .SingleOrDefaultAsync(x => x.ProductId == productId);

            if (p == null)
                throw new KeyNotFoundException($"Product {productId} not found.");

            var history = p.PriceHistory
                .OrderByDescending(h => h.Timestamp)
                .Select(h => new DbProductHistoryEntry
                {
                    ProductHistoryEntryId = h.ProductPriceHistoryId,
                    ProductId = h.ProductId,
                    Price = h.NewPrice,
                    Date = h.Timestamp
                })
                .ToList();

            return new DbProductHistory
            {
                ProductHistoryId = p.ProductId,
                Name = p.Name,
                ProductHistory = history
            };
        }

        public async Task SetDiscountPriceAsync(int productId, decimal discount)
        {
            var product = await _db.Products.FindAsync(productId)
                          ?? throw new KeyNotFoundException($"Product {productId} not found.");

            var existing = await _db.ProductDiscounts
                .SingleOrDefaultAsync(d => d.ProductId == productId);

            if (existing == null)
            {
                _db.ProductDiscounts.Add(new EFProductDiscount
                {
                    ProductId = productId,
                    DiscountPercentage = discount
                });
            }
            else
            {
                existing.DiscountPercentage = discount;
                _db.ProductDiscounts.Update(existing);
            }

            product.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<bool> UpdateProductAsync(DbProduct dbProd)
        {
            var p = await _db.Products.FindAsync(dbProd.ProductId);
            if (p == null) return false;

            p.Name = dbProd.Name;
            p.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePriceAsync(int productId, decimal price)
        {
            var p = await _db.Products.FindAsync(productId);
            if (p == null) 
                return false;

            var oldPrice = p.Price;
            p.Price = price;

            _db.ProductPriceHistories.Add(new EFProductPriceHistory
            {
                ProductId = productId,
                Timestamp = DateTime.UtcNow,
                OldPrice = oldPrice,
                NewPrice = price
            });

            p.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<DbProductDiscount>> GetDiscountsAsync()
        {
            return await _db.ProductDiscounts
                            .Select(p => new DbProductDiscount
                            {
                                ProductId=p.ProductId,
                                DiscountPercentage = p.DiscountPercentage,
                                ProductDiscountId = p.ProductDiscountId
                            })
                            .ToListAsync();
        }

        public async Task<DbProductDiscount> GetDiscountsForProductAsync(int productId)
        {
            var efProd = await _db.ProductDiscounts.SingleOrDefaultAsync(d => d.ProductId == productId);
            if (efProd == null)
                return null;

            return new DbProductDiscount() {  ProductId = efProd.ProductId, ProductDiscountId = efProd.ProductDiscountId, DiscountPercentage = efProd.DiscountPercentage };
        }
    }
}