using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Data
{
    public interface IUserDataCachingService
    {
        Task<ServiceResult> CacheTransactions(string userId, ICollection<Transaction> transactions, TimeSpan lifespan);
        Task<ServiceObjectResult<ICollection<Transaction>>> RetrieveTransactions(string userId);
        Task<ServiceResult> ClearUserData(string userId);
    }
}