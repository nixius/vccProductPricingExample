using Microsoft.EntityFrameworkCore;
using VCC.ProductPricingApiTest.Models.EFDataAccess;

namespace VCC.ProductPricingApiTest.DataAccess
{

    public class EFProductDbContext : DbContext
    {
        public DbSet<EFProduct> Products => Set<EFProduct>();
        public DbSet<EFProductPriceHistory> ProductPriceHistories => Set<EFProductPriceHistory>();
        public DbSet<EFProductDiscount> ProductDiscounts => Set<EFProductDiscount>();

        public EFProductDbContext(DbContextOptions<EFProductDbContext> options) : 
            base(options) 
        { 
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<EFProduct>(e =>
            {
                e.HasKey(x => x.ProductId);
                e.Property(x => x.Name).IsRequired().HasMaxLength(128);
            });

            b.Entity<EFProductPriceHistory>(e =>
            {
                e.HasKey(x => x.ProductPriceHistoryId);
                e.HasOne(x => x.Product)
                 .WithMany(p => p.PriceHistory)
                 .HasForeignKey(x => x.ProductId);
            });

            b.Entity<EFProductDiscount>(e =>
            {
                e.HasKey(x => x.ProductDiscountId);
                e.HasOne(x => x.Product)
                 .WithOne(p => p.Discount)
                 .HasForeignKey<EFProductDiscount>(x => x.ProductId);
                e.HasIndex(x => x.ProductId).IsUnique();
            });
        }

    }

}
