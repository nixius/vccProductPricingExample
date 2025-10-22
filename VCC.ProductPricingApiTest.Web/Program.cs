using VCC.ProductPricingApiTest.Web.Components;
using MudBlazor.Services;
using VCC.ProductPricingApiTest.DataAccess;
using VCC.ProductPricingApiTest.BLL;

namespace VCC.ProductPricingApiTest.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpClient("api", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5213");
            });

            builder.Services
                .AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));
            builder.Services.AddMudServices();

            builder.Services.AddScoped<IProductDataAccess, StaticProductDataAccess>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IPriceService, PriceService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
             
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
