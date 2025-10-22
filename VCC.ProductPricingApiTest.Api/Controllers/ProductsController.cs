using Microsoft.AspNetCore.Mvc;
using VCC.ProductPricingApiTest.BLL;
using VCC.ProductPricingApiTest.Models.Api;

namespace VCC.ProductPricingApiTest.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        private readonly IProductService _productService;
        private readonly IPriceService _priceService;

        public ProductsController(ILogger<ProductsController> logger, IProductService productService, IPriceService priceService)
        {
            _logger = logger;
            _productService = productService;
            _priceService = priceService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ApiProduct>>> Get()
        {
            var products = await _productService.GetProductsAsync();
            var discounts = await _productService.GetDiscountsAsync();

            foreach(var d in discounts)
            {
                var discProd = products.SingleOrDefault(p => p.Id == d.Id);
                if (discProd == null)
                    continue;

                discProd.CurrentPrice = _priceService.GetDiscountPrice(discProd.OriginalPrice, d.DiscountPercentage);
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiProductHistory>> Get(int id)
        {
            return Ok(await _productService.GetProductHistoryByIdAsync(id));
        }

        [HttpPost("{id}/apply-discount")]
        public async Task<ActionResult<ApplyProductDiscountResponse>> ApplyDiscount(int id, [FromBody] ApplyProductDiscountRequest discountRequest)
        {
            var discountedProd = await _productService.SetDiscountPriceAsync(id, discountRequest.DiscountPercentage);
            if(discountedProd == null)
                return BadRequest();

            var discountPrice = _priceService.GetDiscountPrice(discountedProd.OriginalPrice, discountRequest.DiscountPercentage);

            return Ok(new ApplyProductDiscountResponse()
            {
                Id = discountedProd.Id,
                Name = discountedProd.Name,
                OriginalPrice = discountedProd.OriginalPrice,
                DiscountedPrice = discountPrice
            });
        }

        [HttpPut("{id}/update-price")]
        public async Task<ActionResult<UpdateProductPriceResponse>> UpdatePrice(int id, [FromBody] UpdateProductPriceRequest updateRequest)
        {
            var updatedPriceProd = await _productService.UpdatePriceAsync(id, updateRequest.NewPrice);

            if(updatedPriceProd == null)
                return BadRequest();

            var originalPrice = updateRequest.NewPrice;
            decimal? currentPrice = null;

            var prodDiscount = await _productService.GetDiscountForProductAsync(id);
            if(prodDiscount != null)
            {
                currentPrice = _priceService.GetDiscountPrice(originalPrice, prodDiscount.DiscountPercentage);
            }

            return Ok(new UpdateProductPriceResponse()
            {
                Id = updatedPriceProd.Id,
                Name = updatedPriceProd.Name,
                OriginalPrice = originalPrice,
                NewPrice = currentPrice,
                LastUpdatedUtc = updatedPriceProd.LastUpdatedUtc
            });
        }
    }
}