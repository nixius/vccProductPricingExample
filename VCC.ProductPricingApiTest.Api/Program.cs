using Microsoft.EntityFrameworkCore;
using VCC.ProductPricingApiTest.BLL;
using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.Models.DataAccess;
using VCC.ProductPricingApiTest.Models.EFDataAccess;

namespace VCC.ProductPricingApiTest.Api
{
    public class Program
    {
        public enum DataAccessMode
        {
            Invalid = -1,
            StaticData = 1,
            Dapper = 2,
            EF = 3
        }

        public static async Task Main(string[] args)
        {
            var dataAccessMode = DataAccessMode.EF;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();


            switch (dataAccessMode)
            {

                case DataAccessMode.StaticData: 
                    builder.Services.AddScoped<IProductDataAccess, StaticProductDataAccess>();
                    break;
                case DataAccessMode.EF:
                    builder.Services.AddDbContext<EFProductDbContext>(opt => opt.UseInMemoryDatabase("EFProductDb"));
                    builder.Services.AddScoped<IProductDataAccess, EFProductDataAccess>();
                    break;
                default:
                    throw new Exception("No valid data access mode set");
            }

            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IPriceService, PriceService>();

            var app = builder.Build();

            switch (dataAccessMode)
            {

                case DataAccessMode.StaticData: SeedStaticData(); break;
                case DataAccessMode.EF:
                    using (var scope = app.Services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<EFProductDbContext>();
                        await SeedEFData(db);
                    }
                    break;
                default:
                    throw new Exception("No valid data access mode set");
            }

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UsePathBase("/api");

            app.MapControllers();



            app.Run();
        }

        public static void SeedStaticData()
        {
            var prods = new List<DbProduct>()
            {
                new DbProduct() { ProductId = 1, Name = "Assorted Gold coins", Price = 100.0m, LastUpdatedUtc = DateTime.UtcNow },
                new DbProduct() { ProductId = 2, Name = "Atari Jaguar (Non-working)", Price = 200.0m, LastUpdatedUtc = DateTime.UtcNow },
                new DbProduct() { ProductId = 3, Name = "Vase", Price = 300.0m, LastUpdatedUtc = DateTime.UtcNow }
            };

            var hist = new List<DbProductHistoryEntry>()
            {
                new DbProductHistoryEntry() { ProductHistoryEntryId = 1, ProductId = 1, Price = 100, Date = DateTime.UtcNow.AddMinutes(-5) },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 2, ProductId = 1, Price = 110, Date = DateTime.UtcNow.AddMinutes(-4) },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 3, ProductId = 2, Price = 200, Date = DateTime.UtcNow.AddMinutes(-3) },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 4, ProductId = 2, Price = 210, Date = DateTime.UtcNow.AddMinutes(-2) },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 5, ProductId = 3, Price = 300, Date = DateTime.UtcNow.AddMinutes(-1)},
                new DbProductHistoryEntry() { ProductHistoryEntryId = 6, ProductId = 3, Price = 150, Date = DateTime.UtcNow, DiscountPercentage = 50m},

            };

            var discounts = new List<DbProductDiscount>()
            {
                new DbProductDiscount() { ProductDiscountId = 1, DiscountPercentage = 50, ProductId = 3}
            };

            StaticProductDbContext.Instance.InitialiseData(prods, hist, discounts);
        }

        public static async Task SeedEFData(EFProductDbContext db)
        {
            // Don't ever re-seed
            if (await db.Products.AnyAsync()) 
                return;

            var p1 = new EFProduct { Name = "French Franks (1940)", LastUpdated = DateTime.UtcNow, Price = 100m };
            var p2 = new EFProduct { Name = "Casio Cassette player", LastUpdated = DateTime.UtcNow, Price = 200m };
            var p3 = new EFProduct { Name = "1850's Encyclopedia Collection", LastUpdated = DateTime.UtcNow, Price = 300m };

            db.Products.AddRange(p1, p2, p3);

            db.ProductPriceHistories.AddRange(
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 100m },
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 110m },
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 120m },
                            new EFProductPriceHistory { Product = p2, Timestamp = DateTime.UtcNow, NewPrice = 200m },
                            new EFProductPriceHistory { Product = p2, Timestamp = DateTime.UtcNow, NewPrice = 210m },
                            new EFProductPriceHistory { Product = p3, Timestamp = DateTime.UtcNow, NewPrice = 300m },
                            new EFProductPriceHistory { Product = p3, Timestamp = DateTime.UtcNow, NewPrice = 150m, DiscountPercentage = 50 }

                        );

            db.ProductDiscounts.Add(
                new EFProductDiscount { Product = p3, DiscountPercentage = 50m }
            );

            await db.SaveChangesAsync();
        }
    }
}