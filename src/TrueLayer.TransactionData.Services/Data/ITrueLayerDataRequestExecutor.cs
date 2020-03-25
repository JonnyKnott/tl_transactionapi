using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Data
{
    
    public interface ITrueLayerDataRequestExecutor
    {
        Task<ServiceObjectResult<ICollection<Account>>> GetAccounts(string accessToken);
        Task<ServiceObjectResult<ICollection<Transaction>>> GetTransactions(string accessToken, string accountId);
        Task<ServiceObjectResult<AuthResponse>> RefreshAccountAccess(AccountAccessContext accountContext);
        Task<ServiceObjectResult<AuthResponse>> ExchangeCode(AccountAccessContext accountContext);

    }
}