using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Auth
{
    public interface IAuthorizationService
    {
        Task<ServiceObjectResult<AccountAccessContext>> RefreshAccountContext(AccountAccessContext accountContext);
        Task<ServiceObjectResult<AccountAccessContext>> ExchangeCode(AccountAccessContext accountContext);
    }
}