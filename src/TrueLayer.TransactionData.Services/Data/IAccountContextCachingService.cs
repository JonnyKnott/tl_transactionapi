using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Data
{
    public interface IAccountContextCachingService
    {
        Task<ServiceResult> CacheNewAccountContext(string accessCode, ICollection<Scope> scopes);
        Task<ServiceResult> UpdateAccountContext(AccountAccessContext accessContext);
        Task<ServiceObjectResult<ICollection<AccountAccessContext>>> RetrieveActiveAccountsFromCache();
    }
}