using VCC.ProductPricingApiTest.BLL;

namespace VCC.ProductPricingApiTest.Tests.ServiceTests
{
    public class PriceServiceTests
    {
        private readonly IPriceService _priceService;

        public PriceServiceTests()
        {
            _priceService = IOCHelper.Instance.GetService<IPriceService>();
        }


        [TestCase(100,10,90)]       // general good
        [TestCase(200, 25, 150)]    // general good
        [TestCase(100, 10, 90)]    // general good
        [TestCase(100, 0, 100)]     // <= 0 returns original
        [TestCase(100, -1, 100)]    // <= 0 returns original
        [TestCase(80, 99.5, 0.40)]  // high bounds check
        [TestCase(19.99, 25, 14.99)]  // realistc rounding for a retail product
        [TestCase(100, 33.335, 66.67)]  // round away from zero testing
        [TestCase(0.05, 50, 0.03)]  // very small price; 0.025 → 0.03 AwayFromZero
        [TestCase(999999.99, 12.5, 874999.99)] // large value
        public void GetDiscountPrice_ReturnsExpectedPrices(decimal originalPrice, decimal discountPerc, decimal expectedOutput)
        {
            var newPrice = _priceService.GetDiscountPrice(originalPrice, discountPerc);
            Assert.That(newPrice, Is.EqualTo(expectedOutput));
        }

        [TestCase(100, 100)]
        [TestCase(100, 100.0001)]
        public void GetDiscountPrice_InvalidInputs_FailsGracefully(decimal originalPrice, decimal discountPerc)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _priceService.GetDiscountPrice(originalPrice, discountPerc));
        }
    }
}
