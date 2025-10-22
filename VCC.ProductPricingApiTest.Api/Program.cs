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
                new DbProduct() { ProductId = 1, Name = "A", Price = 100.0m, LastUpdatedUtc = DateTime.UtcNow },
                new DbProduct() { ProductId = 2, Name = "B", Price = 200.0m, LastUpdatedUtc = DateTime.UtcNow },
                new DbProduct() { ProductId = 3, Name = "C", Price = 300.0m, LastUpdatedUtc = DateTime.UtcNow }
            };

            var hist = new List<DbProductHistoryEntry>()
            {
                new DbProductHistoryEntry() { ProductHistoryEntryId = 1, ProductId = 1, Price = 100, Date = DateTime.UtcNow },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 2, ProductId = 1, Price = 110, Date = DateTime.UtcNow },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 3, ProductId = 2, Price = 200, Date = DateTime.UtcNow },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 4, ProductId = 2, Price = 210, Date = DateTime.UtcNow },
                new DbProductHistoryEntry() { ProductHistoryEntryId = 5, ProductId = 3, Price = 300, Date = DateTime.UtcNow }
            };

            var discounts = new List<DbProductDiscount>()
            {
                new DbProductDiscount() {ProductDiscountId = 1, ProductId = 3, DiscountPercentage = 10m }
            };

            StaticProductDbContext.Instance.InitialiseData(prods, hist, discounts);
        }

        public static async Task SeedEFData(EFProductDbContext db)
        {
            // Don't ever re-seed
            if (await db.Products.AnyAsync()) 
                return;

            var p1 = new EFProduct { Name = "A", LastUpdated = DateTime.UtcNow, Price = 100m };
            var p2 = new EFProduct { Name = "B", LastUpdated = DateTime.UtcNow, Price = 200m };
            var p3 = new EFProduct { Name = "C", LastUpdated = DateTime.UtcNow, Price = 300m };

            db.Products.AddRange(p1, p2, p3);

            db.ProductPriceHistories.AddRange(
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 100m },
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 110m },
                            new EFProductPriceHistory { Product = p1, Timestamp = DateTime.UtcNow, NewPrice = 120m },
                            new EFProductPriceHistory { Product = p2, Timestamp = DateTime.UtcNow, NewPrice = 200m },
                            new EFProductPriceHistory { Product = p2, Timestamp = DateTime.UtcNow, NewPrice = 210m },
                            new EFProductPriceHistory { Product = p3, Timestamp = DateTime.UtcNow, NewPrice = 300m }
                        );

            db.ProductDiscounts.Add(
                new EFProductDiscount { Product = p3, DiscountPercentage = 10m }
            );

            await db.SaveChangesAsync();
        }
    }
}