namespace VCC.ProductPricingApiTest.BLL
{
    public interface IPriceService
    {
        decimal GetDiscountPrice(decimal originalPrice, decimal discountPerc);
    }
}