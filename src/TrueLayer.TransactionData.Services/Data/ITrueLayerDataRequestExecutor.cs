using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Data
{
    
    public interface ITrueLayerDataRequestExecutor
    {
        Task<ServiceObjectResult<TrueLayerListResponse<Account>>> GetAccounts(string accessToken);
        Task<ServiceObjectResult<TrueLayerListResponse<Transaction>>> GetTransactions(string accessToken, string accountId);
        Task<ServiceObjectResult<AuthResponse>> RefreshAccountAccess(AccountAccessContext accountContext);
        Task<ServiceObjectResult<AuthResponse>> ExchangeCode(AccountAccessContext accountContext);
        Task<ServiceObjectResult<TrueLayerListResponse<AccessTokenMetadata>>> GetAccessTokenMetadata(string accessToken);

    }
}