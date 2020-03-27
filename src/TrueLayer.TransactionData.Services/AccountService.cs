using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.Services
{
    public class AccountService : IAccountService
    {
        private readonly ITrueLayerDataRequestExecutor _requestExecutor;

        public AccountService(ITrueLayerDataRequestExecutor requestExecutor)
        {
            _requestExecutor = requestExecutor;
        }

        public async Task<ServiceObjectResult<ICollection<Account>>> GetAccountsInContext(
            AccountAccessContext accessContext)
        {
            var accountsInContextResult = await _requestExecutor.GetAccounts(accessContext.AccessToken);

            if (!accountsInContextResult.Success || accountsInContextResult.Result.Status != "Succeeded")
                return ServiceObjectResult<ICollection<Account>>.Failed(null,
                    accountsInContextResult.Errors ?? new List<string>
                        {"An error occurred fetching account information"});
            
            return ServiceObjectResult<ICollection<Account>>.Succeeded(accountsInContextResult.Result.Results);
        }
    }
}