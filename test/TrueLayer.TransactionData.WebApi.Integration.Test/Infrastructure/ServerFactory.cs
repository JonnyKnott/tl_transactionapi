using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TrueLayer.TransactionData.WebApi.Test.Infrastructure
{
    public class ServerFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup: class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration.
                // var descriptor = services.SingleOrDefault(
                //     d => d.ServiceType ==
                //          typeof(DbContextOptions<ApplicationDbContext>));
                //
                // if (descriptor != null)
                // {
                //     services.Remove(descriptor);
                // }

                
            });
        }
    }
}