using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.Models.DataAccess;

namespace VCC.ProductPricingApiTest.Tests.DataAccessTests
{
    public class StaticProductDataAccessTests
    {
        private readonly IProductDataAccess _productDb;

        public StaticProductDataAccessTests()
        {
            _productDb = IOCHelper.Instance.GetKeyedService<IProductDataAccess>("static");
        }

        [OneTimeSetUp]
        public void Initialise()
        {
            var prods = new List<DbProduct>()
            {
                new DbProduct() { ProductId = 1, Name = "A", Price = 100.0m },
                new DbProduct() { ProductId = 2, Name = "B", Price = 200.0m },
                new DbProduct() { ProductId = 3, Name = "C", Price = 300.0m }
            };

            var hist = new List<DbProductHistoryEntry>()
            {
                new DbProductHistoryEntry() { ProductHistoryEntryId = 1, ProductId = 1, Price = 100, Date = DateTime.UtcNow, DiscountPercentage = 54.0m },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 2, ProductId = 1, Price = 110, Date = DateTime.UtcNow },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 3, ProductId = 2, Price = 200, Date = DateTime.UtcNow, DiscountPercentage = 10.0m },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 4, ProductId = 2, Price = 210, Date = DateTime.UtcNow },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 5, ProductId = 3, Price = 300, Date = DateTime.UtcNow }
            };

            var discounts = new List<DbProductDiscount>()
            {
                new DbProductDiscount() {ProductDiscountId = 1, ProductId = 3, DiscountPercentage = 10m }
            };

            StaticProductDbContext.Instance.InitialiseData(prods, hist, discounts);
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
        public async Task CanGetDiscounts()
        {
            var result = await _productDb.GetDiscountsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ProductId, Is.EqualTo(3));
            Assert.That(result[0].DiscountPercentage, Is.EqualTo(10));
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
            Assert.That(result.ProductHistory[0].Price, Is.EqualTo(200));
            Assert.That(result.ProductHistory[0].DiscountPercentage, Is.EqualTo(10));
            Assert.That(result.ProductHistory[1].Price, Is.EqualTo(210));
        }

        [Test]
        public async Task UpdateProduct_CompletesSuccesfully()
        {
            var newProd = await _productDb.UpdateProductAsync(new DbProduct() { ProductId = 3, Name = "NewProd" });
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

        [Test]
        public async Task LogDiscountPriceHistoryAsync_CompletesSuccesfully()
        {
            await _productDb.LogDiscountPriceHistoryAsync(1, 55, 123.45m, 789.01m);
        }
    }
}
