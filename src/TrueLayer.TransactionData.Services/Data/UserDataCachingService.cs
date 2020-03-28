using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data.Extensions;

namespace TrueLayer.TransactionData.Services.Data
{
    public class UserDataCachingService : IUserDataCachingService
    {
        private readonly IDistributedCache _cache;

        public UserDataCachingService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<ServiceResult> CacheTransactions(string userId, ICollection<Transaction> transactions, TimeSpan lifespan)
        {
            await _cache.SetValueObject($"{CacheKeys.TransactionCollectionKeyPrefix}{userId}", transactions, lifespan);
            
            return ServiceResult.Succeeded();
        }

        public async Task<ServiceObjectResult<ICollection<Transaction>>> RetrieveTransactions(string userId)
        {
            var transactionsForUser = await _cache.Get<List<Transaction>>($"{CacheKeys.TransactionCollectionKeyPrefix}{userId}");
            
            return ServiceObjectResult<ICollection<Transaction>>.Succeeded(transactionsForUser);
        }

        public async Task<ServiceResult> ClearUserData(string userId)
        {
            await _cache.RemoveAsync($"{CacheKeys.TransactionCollectionKeyPrefix}{userId}");
            
            return ServiceResult.Succeeded();
        }
    }
}