using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data.Extensions;

namespace TrueLayer.TransactionData.Services.Data
{
    public class AccessContextCachingService : IAccessContextCachingService
    {
        private readonly IDistributedCache _cache;

        public AccessContextCachingService(IDistributedCache cache)
        {
            _cache = cache;
        }
        
        public async Task<ServiceObjectResult<ICollection<AccountAccessContext>>> RetrieveAccessContexts(string userId, ICollection<Scope> requiredScopes = null)
        {
            var accessContexts = await _cache.Get<List<AccountAccessContext>>(userId);
            
            return requiredScopes == null ?  
                ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(accessContexts)
                : ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(
                    accessContexts.Where(x => x.Scopes.Distinct().Intersect(requiredScopes.Distinct()).Count() == requiredScopes.Distinct().Count()).ToList());
        }

        public async Task<ServiceResult> PutAccessContext(AccountAccessContext accessContext)
        {
            var userAccessContexts = await _cache.Get<List<AccountAccessContext>>(accessContext.UserId);

            if (userAccessContexts == null)
            {
                userAccessContexts = new List<AccountAccessContext>();
            }

            var existingAccessContext = userAccessContexts.FirstOrDefault(x =>
                x.UserId == accessContext.UserId &&
                x.ProviderName == accessContext.ProviderName);

            if (existingAccessContext != null)
                userAccessContexts.Remove(existingAccessContext);

            userAccessContexts.Add(accessContext);

            await _cache.SetValueObject(accessContext.UserId, userAccessContexts);

            return ServiceResult.Succeeded();
        }

        public async Task<ServiceResult> RemoveAccessContext(AccountAccessContext accessContext)
        {
            var userAccessContexts = await _cache.Get<List<AccountAccessContext>>(accessContext.UserId);

            if(userAccessContexts == null)
                return ServiceResult.Succeeded();
            
            var existingAccessContext = userAccessContexts.FirstOrDefault(x =>
                x.UserId == accessContext.UserId &&
                x.ProviderName == accessContext.ProviderName);

            if (existingAccessContext == null)
                return ServiceResult.Succeeded();
            
            userAccessContexts.Remove(existingAccessContext);
            
            await _cache.SetValueObject(accessContext.UserId, userAccessContexts);
            
            return ServiceResult.Succeeded();
        }
    }
}