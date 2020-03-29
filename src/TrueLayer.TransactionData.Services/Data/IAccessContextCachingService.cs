using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Data
{
    public interface IAccessContextCachingService
    {
        Task<ServiceObjectResult<ICollection<AccountAccessContext>>> RetrieveAccessContexts(string userId, ICollection<Scope> requiredScopes = null);
        Task<ServiceResult> PutAccessContext(AccountAccessContext accessContext);
        Task<ServiceResult> RemoveAccessContext(AccountAccessContext accessContext);
    }
}