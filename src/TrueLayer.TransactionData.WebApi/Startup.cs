using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TrueLayer.TransactionData.Models;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Configurations;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionConfiguration = _configuration.GetSection("Connections").Get<ConnectionConfiguration>();
            var trueLayerConfiguration = _configuration.GetSection("TrueLayer").Get<TrueLayerConfiguration>();
            trueLayerConfiguration.ClientSecret = _configuration["TrueLayer:ClientSecret"];
            
            services
                .AddScoped<ICallbackService, CallbackService>()
                .AddScoped<IAccountContextCachingService, AccountContextCachingService>()
                .AddScoped<ITransactionService, TransactionService>()
                .AddScoped<IAccountService, AccountService>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddScoped<ITrueLayerDataRequestExecutor, TrueLayerDataRequestExecutor>()
                .AddScoped<IRequestTypeEndpointService, RequestTypeEndpointService>()
                .AddSingleton(trueLayerConfiguration);

            services
                .AddHttpClient(HttpClients.TrueLayerDataClientName,
                    client => { client.BaseAddress = new Uri(trueLayerConfiguration.DataApiUri); });
            
            services
                .AddHttpClient(HttpClients.TrueLayerAuthClientName,
                    client => { client.BaseAddress = new Uri(trueLayerConfiguration.AuthApiUri); });
            
            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = connectionConfiguration.RedisEndpoint;
                    options.InstanceName = "master";
                });

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/ping", new HealthCheckOptions
            {
                AllowCachingResponses = false,
                ResultStatusCodes = new Dictionary<HealthStatus, int>
                {
                    { HealthStatus.Healthy, StatusCodes.Status200OK },
                    { HealthStatus.Degraded, StatusCodes.Status200OK },
                    { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable }

                }
            });
            
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}