using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using VCC.ProductPricingApiTest.BLL;
using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.Models.Api;
using VCC.ProductPricingApiTest.Models.DataAccess;

namespace VCC.ProductPricingApiTest.Tests.ServiceTests
{
    public class ProductServiceTests
    {
        private readonly IProductDataAccess _db;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _db = Substitute.For<IProductDataAccess>();
            _productService = new ProductService(_db);
        }

        [TestCase(1, 1)]
        [TestCase(2, 400)]
        [TestCase(3, 999.99)]
        public async Task UpdatePriceAsync_valiParams_ReturnsExpected(int productId, decimal newValue)
        {
            _db.UpdatePriceAsync(productId, newValue, null).Returns(true);
            _db.GetProductByIdAsync(productId).Returns(new DbProduct()
            {
                ProductId = productId,
                LastUpdatedUtc = DateTime.UtcNow,
                Name = "NA",
                Price = newValue
            });

            var updatePriceResult = await _productService.UpdatePriceAsync(productId, newValue);

            Assert.That(updatePriceResult, Is.Not.Null);
            Assert.That(updatePriceResult, Is.TypeOf<ApiProduct>());
            Assert.That(updatePriceResult.Id, Is.EqualTo(productId));
            Assert.That(updatePriceResult.OriginalPrice, Is.EqualTo(newValue));

            await _db.Received(1).UpdatePriceAsync(productId, newValue, null);
            await _db.Received(1).GetProductByIdAsync(productId);
        }

        [TestCase(-1, 10)]
        [TestCase(0, 10)]
        [TestCase(1, -1)]
        [TestCase(1, 0)]
        public async Task UpdatePriceAsync_invaliParams_ReturnsExpected(int productId, decimal newValue)
        {
            var updatePriceResult = await _productService.UpdatePriceAsync(productId, newValue);
            Assert.That(updatePriceResult, Is.Null);

        }
    }
}