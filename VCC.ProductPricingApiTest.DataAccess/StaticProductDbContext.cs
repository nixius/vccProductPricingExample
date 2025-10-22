using VCC.ProductPricingApiTest.Models.DataAccess;

namespace VCC.ProductPricingApiTest.DataAccess
{
    public class StaticProductDbContext
    {
        private static StaticProductDbContext? _instance;
        private static readonly object _lock = new();

        private static List<DbProduct> InMemoryProductRepos = new List<DbProduct>();
        private static List<DbProductHistoryEntry> InMemoryPriceHistoryRepos = new List<DbProductHistoryEntry>();
        private static List<DbProductDiscount> InMemoryDiscountRepos = new List<DbProductDiscount>();

        private StaticProductDbContext() { }

        public static StaticProductDbContext Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;

                lock (_lock)
                {
                    _instance ??= new StaticProductDbContext();
                    return _instance;
                }
            }
        }

        public DbProduct AddProduct(DbProduct product)
        {
            // we aren't going to try and insert at a specific id or without a price
            if (product.ProductId > 0 || product.Price <= 0.0m)
                return null;

            lock(_lock)
            {
                // if the name already exists, return null
                if(InMemoryProductRepos.Any(pr => pr.Name == product.Name))
                {
                    throw new ArgumentException("A value with this name already exists in the product repos");
                }

                var newProdId = InMemoryProductRepos.Any() ? InMemoryProductRepos.Max(pr => pr.ProductId) + 1 : 1;
                product.ProductId = newProdId;
                product.LastUpdatedUtc = DateTime.UtcNow;

                InMemoryProductRepos.Add(product);

                var newHistId = InMemoryPriceHistoryRepos.Any() ? InMemoryPriceHistoryRepos.Max(ph => ph.ProductHistoryEntryId) + 1 : 1;
                InMemoryPriceHistoryRepos.Add(new DbProductHistoryEntry() { ProductHistoryEntryId = newHistId, Date = DateTime.UtcNow, Price = product.Price, ProductId = newProdId });

                return product;
            }
        }

        public bool UpdateProduct(int productId, string? name = null, decimal? price = null)
        {
            // we can't update without valid date
            if (productId <= 0 || (string.IsNullOrEmpty(name) && !price.HasValue))
                return false;

            lock (_lock)
            {
                // can we find the product at all?
                var dbProd = InMemoryProductRepos.SingleOrDefault(pr => pr.ProductId == productId);
                if (dbProd == null)
                    return false;


                if(!string.IsNullOrWhiteSpace(name))
                    dbProd.Name = name;

                if (price.HasValue && price.Value != dbProd.Price)
                {
                    dbProd.Price = price.Value;

                    var newHistId = InMemoryPriceHistoryRepos.Max(ph => ph.ProductHistoryEntryId) + 1;
                    InMemoryPriceHistoryRepos.Add(new DbProductHistoryEntry() { ProductHistoryEntryId = newHistId, Date = DateTime.UtcNow, Price = price.Value, ProductId = productId });
                }

                dbProd.LastUpdatedUtc = DateTime.UtcNow;

                return true;
            }
        }

        public List<DbProduct> GetProducts()
        {
            var retProds = new List<DbProduct>();

            lock (_lock)
            {
                foreach (var p in InMemoryProductRepos)
                {
                    retProds.Add(new DbProduct()
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        LastUpdatedUtc = p.LastUpdatedUtc,
                        Price = InMemoryPriceHistoryRepos.Where(x => x.ProductId == p.ProductId)!  // A price _must_ be inserted when adding a product, so we can assume it exists
                                                          .OrderByDescending(y => y.Date)
                                                          .First()
                                                          .Price
                    });
                }

                return retProds;
            }
        }

        public DbProduct GetProduct(int productId)
        {
            lock (_lock)
            {
                var prod = InMemoryProductRepos.SingleOrDefault(pr => pr.ProductId == productId);
                if (prod == null)
                    return null;

                prod.Price = InMemoryPriceHistoryRepos.Where(x => x.ProductId == productId)!  // A price _must_ be inserted when adding a product, so we can assume it exists
                                                      .OrderByDescending(y => y.Date)
                                                      .First()
                                                      .Price;

                return prod;
            }
        }

        public DbProductHistory GetProductPriceHistory(int productId)
        {
            var retHist = new DbProductHistory();

            lock (_lock)
            {
                var histObj = InMemoryPriceHistoryRepos.Where(ph => ph.ProductId == productId).ToList();

                if (!histObj.Any())
                    return null;

                retHist.ProductHistoryId = productId;
                retHist.Name = InMemoryProductRepos.Single(pr => pr.ProductId == productId).Name;  // Has to exist, would rather this throw here than return Null with SingleOrDefault()

                foreach (var ho in histObj)
                {
                    retHist.ProductHistory.Add(new DbProductHistoryEntry()
                    {
                        ProductHistoryEntryId = ho.ProductHistoryEntryId,
                        Date = ho.Date,
                        Price = ho.Price,
                        ProductId = productId
                    });
                }
                return retHist;
            }
        }

        public void SetProductDiscount(int productId, decimal discountPerc)
        {
            lock (_lock)
            {
                var discount = InMemoryDiscountRepos.SingleOrDefault(pd => pd.ProductId == productId);
                if (discount == null)
                {
                    var newId = InMemoryDiscountRepos.Any() ? InMemoryDiscountRepos.Max(d => d.ProductDiscountId) + 1 : 1;
                    InMemoryDiscountRepos.Add(new DbProductDiscount() { ProductDiscountId = newId, ProductId = productId, DiscountPercentage = discountPerc });
                }
                else
                {
                    discount.DiscountPercentage = discountPerc;
                }
            }
        }

        public List<DbProductDiscount> GetDiscountsAsync()
        {
            var retProds = new List<DbProductDiscount>();

            lock (_lock)
            {
                InMemoryDiscountRepos.ForEach(dr => retProds.Add(new DbProductDiscount()
                {
                    DiscountPercentage = dr.DiscountPercentage,
                    ProductId = dr.ProductId,
                    ProductDiscountId = dr.ProductDiscountId
                }));
            }

            return retProds;
        }

        public DbProductDiscount GetDiscountsForProductAsync(int productId)
        {
            var retProds = new List<DbProductDiscount>();

            lock (_lock)
            {
                var discount = InMemoryDiscountRepos.FirstOrDefault(d => d.ProductId == productId);
                if (discount == null)
                    return null;

                return new DbProductDiscount() { ProductId = productId, DiscountPercentage = discount.DiscountPercentage, ProductDiscountId = discount.ProductDiscountId };
            }
        }

        public void InitialiseData(List<DbProduct> prods, List<DbProductHistoryEntry> history, List<DbProductDiscount> discounts)
        {
            InMemoryProductRepos = prods;
            InMemoryPriceHistoryRepos = history;
            InMemoryDiscountRepos = discounts;
        }
    }
}