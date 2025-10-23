using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Transactions;
using VCC.ProductPricingApiTest.Models.DataAccess;
using VCC.ProductPricingApiTest.Models.EFDataAccess;

namespace VCC.ProductPricingApiTest.DataAccess
{
    public class DapperProductDataAccess : IProductDataAccess
    {
        private readonly string _connectionString;

        public DapperProductDataAccess(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Server=localhost\\SQLEXPRESS17;Database=vccTest;Trusted_Connection=True;");
        }

        public async Task<DbProduct> AddProductAsync(DbProduct product)
        {
            using var trans = new TransactionScope();
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO Product ([Name], Price) OUTPUT INSERTED.ProductId  VALUES (@productName, @price)");

            var insertId = await conn.ExecuteScalarAsync<int>(sb.ToString(), new { productName = product.Name, price = product.Price });
            product.ProductId = insertId;

            sb = new StringBuilder();
            sb.AppendLine("INSERT INTO ProductPriceHistory (ProductId, [Timestamp], OldPrice, NewPrice) ");
            sb.AppendLine("VALUES (@productId, GETDATE(), NULL, @newPrice)");
            await conn.ExecuteAsync(sb.ToString(), new { productId = insertId, newPrice = product.Price });

            trans.Complete();
            return product;
        }

        public async Task<List<DbProduct>> GetProductsAsync()
        {
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("WITH Latest AS( ");
            sb.AppendLine("    SELECT ");
            sb.AppendLine("        ph.ProductId, ");
            sb.AppendLine("        ph.NewPrice, ");
            sb.AppendLine("        ph.[Timestamp], ");
            sb.AppendLine("        ROW_NUMBER() OVER(PARTITION BY ph.ProductId ORDER BY ph.[Timestamp] DESC) AS rn ");
            sb.AppendLine("    FROM dbo.ProductPriceHistory AS ph ");
            sb.AppendLine(")  ");
            sb.AppendLine("SELECT P.ProductId As Id, P.[Name], P.LastUpdated As LastUpdatedUtc, L.NewPrice AS Price ");
            sb.AppendLine("FROM Product P ");
            sb.AppendLine("LEFT JOIN Latest AS L ON L.ProductId = P.ProductId AND L.RN = 1 ");

            return (await conn.QueryAsync<DbProduct>(sb.ToString())).ToList();
        }

        public async Task<List<DbProductDiscount>> GetDiscountsAsync()
        {
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("SELECT PD.* ");
            sb.AppendLine("FROM ProductDiscount AS D ");

            return (await conn.QueryAsync<DbProductDiscount>(sb.ToString())).ToList();
        }

        public async Task<DbProductDiscount> GetDiscountsForProductAsync(int productId)
        {
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("SELECT PD.* ");
            sb.AppendLine("FROM ProductDiscount AS D ");
            sb.AppendLine("Where ProductId = @productId");

            return await conn.QuerySingleOrDefaultAsync<DbProductDiscount>(sb.ToString(), new { productId });
        }


        public async Task<DbProduct> GetProductByIdAsync(int productId)
        {
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("WITH Latest AS( ");
            sb.AppendLine("    SELECT ");
            sb.AppendLine("        ph.ProductId, ");
            sb.AppendLine("        ph.NewPrice, ");
            sb.AppendLine("        ph.[Timestamp], ");
            sb.AppendLine("        ROW_NUMBER() OVER(PARTITION BY ph.ProductId ORDER BY ph.[Timestamp] DESC) AS rn ");
            sb.AppendLine("    FROM dbo.ProductPriceHistory AS ph ");
            sb.AppendLine(")  ");
            sb.AppendLine("SELECT P.ProductId As Id, P.[Name], P.LastUpdated As LastUpdatedUtc, L.NewPrice AS Price ");
            sb.AppendLine("FROM Product P ");
            sb.AppendLine("LEFT JOIN Latest AS L ON L.ProductId = P.ProductId AND L.RN = 1 ");
            sb.AppendLine("WHERE P.ProductId = @productId");

            return await conn.QuerySingleOrDefaultAsync<DbProduct>(sb.ToString(), new { productId });
        }

        public async Task<DbProductHistory> GetProductHistoryByIdAsync(int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            var sb = new StringBuilder();
            sb.AppendLine("SELECT p.ProductId AS Id, p.[Name] ");
            sb.AppendLine("FROM dbo.Product AS p ");
            sb.AppendLine("WHERE p.ProductId = @productId; ");
            sb.AppendLine("");
            sb.AppendLine("SELECT ");
            sb.AppendLine("    ph.ProductPriceHistoryId AS Id, ");
            sb.AppendLine("    ph.ProductId, ");
            sb.AppendLine("    ph.NewPrice AS Price, ");
            sb.AppendLine("    ph.DiscountPercentage AS DiscountPerc, ");
            sb.AppendLine("    ph.[Timestamp] AS[Date] ");
            sb.AppendLine("FROM dbo.ProductPriceHistory AS ph ");
            sb.AppendLine("WHERE ph.ProductId = @productId ");
            sb.AppendLine("ORDER BY ph.[Timestamp] DESC; ");

            using var multi = await conn.QueryMultipleAsync(sb.ToString(), new { productId });

            var product = (await multi.ReadAsync<DbProductHistory>()).SingleOrDefault();
            if (product == null) 
                return null;

            product.ProductHistory = (await multi.ReadAsync<DbProductHistoryEntry>()).ToList();
            return product;
        }

        public async Task SetDiscountPriceAsync(int productId, decimal discount)
        {
            using var conn = new SqlConnection(_connectionString);
            var sb = new StringBuilder();

            sb.AppendLine("UPDATE dbo.ProductDiscount ");
            sb.AppendLine("SET DiscountPercentage = @discount ");
            sb.AppendLine("WHERE ProductId = @productId; ");
            sb.AppendLine(" ");
            sb.AppendLine("IF @@ROWCOUNT = 0 ");
            sb.AppendLine("BEGIN ");
            sb.AppendLine("    INSERT INTO dbo.ProductDiscount(ProductId, DiscountPercentage) ");
            sb.AppendLine("    VALUES(@productId, @discount); ");
            sb.AppendLine("END ");

            await conn.ExecuteAsync(sb.ToString(), new { productId, discount });
        }

        public async Task LogDiscountPriceHistoryAsync(int productId, decimal discountPerc, decimal prevPrice, decimal newPrice)
        {
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO dbo.ProductPriceHistory (ProductId, [Timestamp], OldPrice, NewPrice, DiscountPercentage) ");
            sb.AppendLine("VALUES (@productId,@timestamp, @prevPrice, @newPrice, @discountPerc);");

            await conn.ExecuteAsync(sb.ToString(), new { productId, discountPerc, prevPrice, newPrice });
        }

        public async Task<bool> UpdateProductAsync(DbProduct dbProd)
        {
            using var conn = new SqlConnection(_connectionString);
            var sb = new StringBuilder();

            sb.AppendLine("UPDATE dbo.Product ");
            sb.AppendLine("SET ");
            sb.AppendLine("    [Name] = @name, ");
            sb.AppendLine("    LastUpdated = @lastUpdated");
            sb.AppendLine("WHERE ProductId = @productId; ");

            return (await conn.ExecuteAsync(sb.ToString(), new { productId = dbProd.ProductId, lastUpdated = DateTime.UtcNow })) == 1;
        }

        public async Task<bool> UpdatePriceAsync(int productId, decimal price)
        {
            using var conn = new SqlConnection(_connectionString);

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO dbo.ProductPriceHistory (ProductId, [Timestamp], OldPrice, NewPrice, DiscountPercentage) ");
            sb.AppendLine("VALUES (@productId,@timestamp, @oldPrice, @price, NULL);");

            var rows = await conn.ExecuteAsync(sb.ToString(), new { productId, price });
            return rows == 1;
        }
    }
}