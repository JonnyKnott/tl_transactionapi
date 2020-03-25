using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data.Extensions;

namespace TrueLayer.TransactionData.Services.Data
{
    public class AccountContextCachingService : IAccountContextCachingService
    {
        private readonly IDistributedCache _cache;

        public AccountContextCachingService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<ServiceResult> CacheNewAccountContext(string accessCode, ICollection<Scope> scopes)
        {
            var accountContext = new AccountAccessContext
            {
                Code = accessCode,
                Scopes = scopes
            };

            var existingAccounts =
                await _cache.Get<List<AccountAccessContext>>(CacheKeys.AccountAccessContextCollection);

            if (existingAccounts != null && existingAccounts.Any())
            {
                if (existingAccounts.Any(x => x.Code == accessCode))
                    return ServiceResult.Succeeded();
                
                existingAccounts.Add(accountContext);
                
                await _cache.SetValueObject(CacheKeys.AccountAccessContextCollection, existingAccounts);
            }
            else
            {
                await _cache.SetValueObject(CacheKeys.AccountAccessContextCollection,
                    new List<AccountAccessContext> {accountContext});
            }

            return ServiceResult.Succeeded();
        }

        public async Task<ServiceResult> UpdateAccountContext(AccountAccessContext accessContext)
        {
            var existingAccountContexts =
                await _cache.Get<List<AccountAccessContext>>(CacheKeys.AccountAccessContextCollection);

            var existingAccountContext = existingAccountContexts.SingleOrDefault(x => x.Code != accessContext.Code);
            
            
            if(existingAccountContext == null)
                throw new ArgumentOutOfRangeException(nameof(accessContext), "No account context exists with this code");

            existingAccountContexts.Remove(existingAccountContext);
            existingAccountContexts.Add(accessContext);

            await _cache.SetValueObject(CacheKeys.AccountAccessContextCollection, existingAccountContexts);
            
            return ServiceResult.Succeeded();
        }

        public async Task<ServiceObjectResult<ICollection<AccountAccessContext>>> RetrieveActiveAccountsFromCache()
        {
            var accountsInCache = await _cache.Get<List<AccountAccessContext>>(CacheKeys.AccountAccessContextCollection);
            
            return ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(accountsInCache);
        }
    }
}