using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrueLayer.TransactionData.Models.ApiModels.JsonConverters;
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
            var connectionConfiguration = _configuration.GetSection("Connections").Get<ConnectionConfiguration>() ?? new ConnectionConfiguration();
            var trueLayerRequestConfiguration = _configuration.GetSection("TrueLayer:Request").Get<TrueLayerRequestConfiguration>() ?? new TrueLayerRequestConfiguration();
            var trueLayerEndpointConfiguration = _configuration.GetSection("TrueLayer:Endpoints").Get<TrueLayerEndpointConfiguration>() ?? new TrueLayerEndpointConfiguration();
            
            services
                .AddScoped<ICallbackService, CallbackService>()
                .AddSingleton<IAccessContextCachingService, AccessContextCachingService>()
                .AddScoped<ITransactionService, TransactionService>()
                .AddScoped<IAccountService, AccountService>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddScoped<ITrueLayerDataRequestExecutor, TrueLayerDataRequestExecutor>()
                .AddSingleton<IRequestTypeEndpointService, RequestTypeEndpointService>()
                .AddSingleton<IUserDataCachingService, UserDataCachingService>()
                .AddSingleton(trueLayerRequestConfiguration)
                .AddSingleton(trueLayerEndpointConfiguration);

            services
                .AddHttpClient(HttpClients.TrueLayerDataClientName,
                    client => { client.BaseAddress = new Uri(trueLayerRequestConfiguration.DataApiUri); });
            
            services
                .AddHttpClient(HttpClients.TrueLayerAuthClientName,
                    client => { client.BaseAddress = new Uri(trueLayerRequestConfiguration.AuthApiUri); });
            
            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });
            
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new TransactionAmountJsonConverter());
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddLogging(builder => { builder.AddConsole(); });

            Console.WriteLine(connectionConfiguration.RedisEndpoint);
            
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