using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TrueLayer.TransactionData.Services;
using TrueLayer.TransactionData.WebApi.Test.Services;

namespace TrueLayer.TransactionData.WebApi.Test.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void ReplaceServiceType<TInterface, TNewImplementation>(this IServiceCollection services)
            where TInterface : class
            where TNewImplementation : class, TInterface
        {
            var registeredService = services.SingleOrDefault(
                service => service.ServiceType == typeof(TInterface));

            services.Remove(registeredService);
            services.AddScoped<TInterface, TNewImplementation>();
        }
    }
}