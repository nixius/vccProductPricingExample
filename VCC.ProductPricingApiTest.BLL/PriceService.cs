namespace VCC.ProductPricingApiTest.BLL
{
    public class PriceService : IPriceService
    {
       public decimal GetDiscountPrice(decimal originalPrice, decimal discountPerc)
       {
            if (discountPerc <= 0)
                return originalPrice;

            if (discountPerc >= 100)
                throw new ArgumentOutOfRangeException("Unable to discountprice by 100% or more");

            var discounted = originalPrice * (1 - (discountPerc / 100.0m));
            var newPrice = Math.Round(discounted, 2, MidpointRounding.AwayFromZero);

            return newPrice;
       }
    }
}