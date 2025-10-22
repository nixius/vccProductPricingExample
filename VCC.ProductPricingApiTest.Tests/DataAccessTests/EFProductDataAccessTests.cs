using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.Models.DataAccess;
using VCC.ProductPricingApiTest.Models.EFDataAccess;

namespace VCC.ProductPricingApiTest.Tests.DataAccessTests
{
    public class EFProductDataAccessTests
    {
        private readonly IProductDataAccess _productDb;
        private readonly EFProductDbContext _dbContext;
        
        public EFProductDataAccessTests()
        {
            _productDb = IOCHelper.Instance.GetKeyedService<IProductDataAccess>("ef");
            _dbContext = IOCHelper.Instance.GetService<EFProductDbContext>();
        }

        [OneTimeSetUp]
        public async Task Initialise()
        {
            var p1 = new EFProduct { Name = "A", LastUpdated = DateTime.UtcNow, Price = 100m };
            var p2 = new EFProduct { Name = "B", LastUpdated = DateTime.UtcNow, Price = 200m };
            var p3 = new EFProduct { Name = "C", LastUpdated = DateTime.UtcNow, Price = 300m };

            _dbContext.Products.AddRange(p1, p2, p3);

            _dbContext.ProductPriceHistories.AddRange(
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 100m },
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 110m },
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 120m },
                            new EFProductPriceHistory { Product = p2, Timestamp = DateTime.UtcNow, NewPrice = 200m },
                            new EFProductPriceHistory { Product = p2, Timestamp = DateTime.UtcNow, NewPrice = 210m },
                            new EFProductPriceHistory { Product = p3, Timestamp = DateTime.UtcNow, NewPrice = 300m }
                        );

            _dbContext.ProductDiscounts.Add(
                new EFProductDiscount { Product = p3, DiscountPercentage = 10m }
            );

            await _dbContext.SaveChangesAsync();


        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task CanAddProducts()
        {
            var result = await _productDb.AddProductAsync(new DbProduct() { Name = "X", Price = 99.99m, LastUpdatedUtc = DateTime.UtcNow });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("X"));
            Assert.That(result.Price, Is.EqualTo(99.99m));
            Assert.That(result.ProductId, Is.GreaterThan(0));
        }

        [Test]
        public async Task CanGetProducts()
        {
            var result = await _productDb.GetProductsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
            Assert.That(result[0].Name, Is.EqualTo("A"));
            Assert.That(result[1].Name, Is.EqualTo("B"));
            Assert.That(result[2].Name, Is.EqualTo("C"));
        }

        [Test]
        public async Task CanGetProductById()
        {
            var result = await _productDb.GetProductByIdAsync(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductId, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("B"));
            Assert.That(result.Price, Is.EqualTo(210.0m));
        }

        [Test]
        public async Task CanGetProductHistoryById()
        {
            var result = await _productDb.GetProductHistoryByIdAsync(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductHistoryId, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("B"));
            Assert.That(result.ProductHistory, Is.Not.Null);
            Assert.That(result.ProductHistory.Count, Is.EqualTo(2));
            Assert.That(result.ProductHistory[0].Price, Is.EqualTo(210m));
            Assert.That(result.ProductHistory[1].Price, Is.EqualTo(200m));
        }

        [Test]
        public async Task UpdateProduct_CompletesSuccesfully()
        {
            var newProd = await _productDb.UpdateProductAsync(new DbProduct() {  ProductId = 3, Name = "NewProd"});
            Assert.That(newProd, Is.True);
        }

        [Test]
        public async Task UpdatePrice_CompletesSuccesfully()
        {
            var newProd = await _productDb.UpdatePriceAsync(3, 55.0m);
            Assert.That(newProd, Is.True);
        }

        [Test]
        public async Task SetDiscount_CompletesSuccesfully()
        {
           await _productDb.SetDiscountPriceAsync(3, 10.0m);
        }
    }
}
