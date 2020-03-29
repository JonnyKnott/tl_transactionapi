using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TrueLayer.TransactionData.Services;
using TrueLayer.TransactionData.WebApi.Test.Services;

namespace TrueLayer.TransactionData.WebApi.Test.Infrastructure
{
    public class ServerFactory : WebApplicationFactory<Startup>
    {
        // override IWebHostBuilder CreateWebHostBuilder()
        // {
        //     return base.CreateWebHostBuilder()
        //         .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
        //         {
        //             new KeyValuePair<string, string>("TrueLayer:Request:DataApiUri", "https://api.truelayer.com"),
        //             new KeyValuePair<string, string>("TrueLayer:Request:AuthApiUri", "https://auth.truelayer.com"),
        //             new KeyValuePair<string, string>("TrueLayer:Request:AccountIdPlaceholder", "{accountId}"),
        //             new KeyValuePair<string, string>("TrueLayer:Request:ClientId", "j0nn7st3stap1-fc947a"),
        //             new KeyValuePair<string, string>("TrueLayer:Request:CallbackUrlBase", "http://localhost:5000/api/v1/callback/"),
        //             new KeyValuePair<string, string>("TrueLayer:Endpoints:AccessTokenMetadataEndpoint", "data/v1/me"),
        //             new KeyValuePair<string, string>("TrueLayer:Endpoints:GetAccountsEndpoint", "data/v1/accounts"),
        //             new KeyValuePair<string, string>("TrueLayer:Endpoints:GetTransactionsEndpoint", "data/v1/accounts/{accountId}/transactions"),
        //             new KeyValuePair<string, string>("TrueLayer:Endpoints:AuthEndpoint", "connect/token")
        //         }));
        // }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseConfiguration(
                new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("TrueLayer:Request:DataApiUri", "TestValue"),
                    new KeyValuePair<string, string>("TrueLayer:Endpoints:AccessTokenMetadataEndpoint", "TestValue")
                }).Build());
            
            builder.ConfigureServices(services =>
            {
                services.ReplaceServiceType<ITransactionService, TestTransactionService>();
                services.ReplaceServiceType<ICallbackService, TestCallbackService>();
            });
        }
    }
}