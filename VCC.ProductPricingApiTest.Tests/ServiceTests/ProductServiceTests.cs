using VCC.ProductPricingApiTest.BLL;

namespace VCC.ProductPricingApiTest.Tests.ServiceTests
{
    public class ProductServiceTests
    {
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _productService = IOCHelper.Instance.GetService<ProductService>();
        }

        // Since all the service is doing is passing through to the Db layer, testing it seems pretty pointless
        // The only thing I can really test is the Convert from DB => Api model function, which will show in integration level testing immediately, so feels overkill
    }
}