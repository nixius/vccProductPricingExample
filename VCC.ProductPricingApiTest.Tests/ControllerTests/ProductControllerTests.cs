using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Collections;
using VCC.ProductPricingApiTest.Api.Controllers;
using VCC.ProductPricingApiTest.BLL;
using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.Models.Api;

namespace VCC.ProductPricingApiTest.Tests.ControllerTests
{
    public class ProductControllerTests
    {
        private IProductService _productService;
        private IPriceService _priceService;
        private ILogger<ProductsController> _logger;

        public ProductControllerTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            _productService = Substitute.For<IProductService>();
            _priceService = Substitute.For<IPriceService>();
            _logger = Substitute.For<ILogger<ProductsController>>();
        }

        [Test]
        public async Task GetProducts_ReturnsSucesfully()
        {
            _productService.GetProductsAsync().Returns(new List<ApiProduct>()
            {
                new ApiProduct() { Id = 1, CurrentPrice = 100, LastUpdatedUtc = DateTime.UtcNow, Name = "A" },
                new ApiProduct() { Id = 2, CurrentPrice = 100, LastUpdatedUtc = DateTime.UtcNow, Name = "B" },
                new ApiProduct() { Id = 2, CurrentPrice = 100, LastUpdatedUtc = DateTime.UtcNow, Name = "C" }
            });

            var pc = new ProductsController(null, _productService, _priceService);

            var controllerResult = await pc.Get();

            Assert.That(controllerResult, Is.Not.Null);
            Assert.That(controllerResult.Result, Is.Not.Null);
            Assert.That(controllerResult, Is.AssignableTo<ActionResult<List<ApiProduct>>>());

            var okResult = controllerResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var model = okResult!.Value as List<ApiProduct>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(3));
            Assert.That(model[0].Name, Is.EqualTo("A"));

            await _productService.Received(1).GetProductsAsync();
        }

        [Test]
        public async Task GetProductHistory_ReturnsSucesfully()
        {
            var history = new ApiProductHistory
            {
                Id = 1,
                Name = "Coins",
                PriceHistory = new List<ApiProductHistoryEntry>()
                {
                    new ApiProductHistoryEntry() { Date =  DateTime.UtcNow, Price = 1234.56m },
                    new ApiProductHistoryEntry() { Date =  DateTime.UtcNow, Price = 6543.21m },
                }
            };

            _productService.GetProductHistoryByIdAsync(5).Returns(history);
            var pc = new ProductsController(null, _productService, _priceService);

            var controllerResult = await pc.Get(5);
            Assert.That(controllerResult, Is.Not.Null);
            Assert.That(controllerResult.Result, Is.Not.Null);
            Assert.That(controllerResult, Is.AssignableTo<ActionResult<ApiProductHistory>>());

            var okResult = controllerResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var model = okResult!.Value as ApiProductHistory;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Id, Is.EqualTo(1));
            Assert.That(model.PriceHistory, Is.Not.Null);
            Assert.That(model.PriceHistory.Count, Is.EqualTo(2));
            Assert.That(model.PriceHistory[0].Price, Is.EqualTo(history.PriceHistory[0].Price));

            await _productService.Received(1).GetProductHistoryByIdAsync(5);
        }

        [TestCase(10,90)]
        [TestCase(50, 50)]
        [TestCase(99, 1)]
        public async Task ApplyDiscount_validDiscounts_ReturnsSucesfully(decimal discount, decimal discountPrice)
        {
            decimal price = 100m;

            _productService.SetDiscountPriceAsync(5, discount).Returns(new ApiProduct()
            {
                Id = 1,
                CurrentPrice = price,
                Name = "A",
                LastUpdatedUtc = DateTime.UtcNow
            });

            _priceService.GetDiscountPrice(price, discount).Returns(discountPrice);

            var pc = new ProductsController(_logger, _productService, _priceService);

            var controllerResult = await pc.ApplyDiscount(5, new ApplyProductDiscountRequest()
            { 
                DiscountPercentage = discount
            });


            Assert.That(controllerResult, Is.Not.Null);
            Assert.That(controllerResult.Result, Is.Not.Null);
            Assert.That(controllerResult, Is.AssignableTo<ActionResult<ApplyProductDiscountResponse>>());

            var okResult = controllerResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var model = okResult!.Value as ApplyProductDiscountResponse;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.DiscountedPrice, Is.EqualTo(discountPrice));
            Assert.That(model.Id, Is.EqualTo(1));
            Assert.That(model.OriginalPrice, Is.EqualTo(price));

            await _productService.Received(1).SetDiscountPriceAsync(5, discount);
            _priceService.Received(1).GetDiscountPrice(price, discount);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(100)]
        [TestCase(101)]

        public async Task ApplyDiscount_invalidDiscounts_ReturnsBadRequest(decimal discount)
        {
            decimal price = 100m;

            _productService.SetDiscountPriceAsync(5, discount).ReturnsNull();

            var pc = new ProductsController(_logger, _productService, _priceService);

            var controllerResult = await pc.ApplyDiscount(5, new ApplyProductDiscountRequest()
            {
                DiscountPercentage = discount
            });


            Assert.That(controllerResult, Is.Not.Null);
            Assert.That(controllerResult.Result, Is.Not.Null);
            Assert.That(controllerResult, Is.AssignableTo<ActionResult<ApplyProductDiscountResponse>>());

            var badRequestResult = controllerResult.Result as BadRequestResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));

            await _productService.Received(1).SetDiscountPriceAsync(5, discount);
        }

        [TestCase(1)]
        [TestCase(999)]
        [TestCase(156.55)]
        public async Task UpdatePrice_validPrice_ReturnsSucesfully(decimal newPrice)
        {

            _productService.UpdatePriceAsync(5, newPrice).Returns(new ApiProduct()
            {
                Id = 5,
                CurrentPrice = newPrice,
                Name = "A",
                LastUpdatedUtc = DateTime.UtcNow
            });

            var pc = new ProductsController(_logger, _productService, _priceService);

            var controllerResult = await pc.UpdatePrice(5,new UpdateProductPriceRequest()
            {
                NewPrice = newPrice
            });


            Assert.That(controllerResult, Is.Not.Null);
            Assert.That(controllerResult.Result, Is.Not.Null);
            Assert.That(controllerResult, Is.AssignableTo<ActionResult<UpdateProductPriceResponse>>());

            var okResult = controllerResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var model = okResult!.Value as UpdateProductPriceResponse;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Id, Is.EqualTo(5));
            Assert.That(model.NewPrice, Is.EqualTo(newPrice));
            Assert.That(model.Name, Is.EqualTo("A"));

            await _productService.Received(1).UpdatePriceAsync(5, newPrice);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public async Task UpdatePrice_invalidPrice_ReturnsSucesfully(decimal newPrice)
        {

            _productService.UpdatePriceAsync(5, newPrice).ReturnsNull();

            var pc = new ProductsController(_logger, _productService, _priceService);

            var controllerResult = await pc.UpdatePrice(5, new UpdateProductPriceRequest()
            {
                NewPrice = newPrice
            });


            Assert.That(controllerResult, Is.Not.Null);
            Assert.That(controllerResult.Result, Is.Not.Null);
            Assert.That(controllerResult, Is.AssignableTo<ActionResult<UpdateProductPriceResponse>>());

            var badRequestResult = controllerResult.Result as BadRequestResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));

            await _productService.Received(1).UpdatePriceAsync(5, newPrice);
        }
    }
}
