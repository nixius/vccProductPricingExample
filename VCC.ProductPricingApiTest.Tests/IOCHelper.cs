using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VCC.ProductPricingApiTest.BLL;
using VCC.ProductPricingApiTest.DataAccess;

namespace VCC.ProductPricingApiTest.Tests
{
    public sealed class IOCHelper
    {
        private static readonly IOCHelper _instance = new IOCHelper();
        private ServiceProvider _serviceProvider;

        public static IOCHelper Instance { get { return _instance; } }

        private IOCHelper()
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            // Set up Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddDbContext<EFProductDbContext>(opt => opt.UseInMemoryDatabase("EFProductDb"));

            serviceCollection.AddKeyedScoped<IProductDataAccess, StaticProductDataAccess>("static");
            serviceCollection.AddKeyedScoped<IProductDataAccess, EFProductDataAccess>("ef");
            serviceCollection.AddKeyedScoped<IProductDataAccess, DapperProductDataAccess>("dapper");

            serviceCollection.AddSingleton<ProductService>();
            serviceCollection.AddSingleton<PriceService>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public T GetKeyedService<T>(string key)
        {
            return _serviceProvider.GetKeyedService<T>(key)!;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>()!;
        }

        public void Dispose()
        {
            if (_serviceProvider != null)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}