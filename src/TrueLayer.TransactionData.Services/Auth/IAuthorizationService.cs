using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Auth
{
    public interface IAuthorizationService
    {
        Task<ServiceResult> AttemptRefreshAccountContexts(string userId);
        Task<ServiceObjectResult<AccountAccessContext>> ExchangeCode(AccountAccessContext accessContext);
    }
}