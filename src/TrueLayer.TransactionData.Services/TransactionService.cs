using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountService _accountService;

        public TransactionService(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<ServiceObjectResult<ICollection<Transaction>>> GetAllTransactions()
        {
            var accountLookupResult = await _accountService.GetAccountsInCurrentContext();
            
            if(!accountLookupResult.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, accountLookupResult.Errors);
            
            return ServiceObjectResult<ICollection<Transaction>>.Succeeded(new List<Transaction>());
        }
    }
}