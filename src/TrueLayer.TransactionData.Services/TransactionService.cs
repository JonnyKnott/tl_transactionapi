using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountService _accountService;
        private readonly ITrueLayerDataRequestExecutor _requestExecutor;
        private readonly IAccessContextCachingService _accessCachingService;
        private readonly IUserDataCachingService _userDataCachingService;
        private readonly IAuthorizationService _authorizationService;

        public TransactionService(
            IAccountService accountService, 
            ITrueLayerDataRequestExecutor requestExecutor,
            IAccessContextCachingService accessCachingService,
            IUserDataCachingService userDataCachingService,
            IAuthorizationService authorizationService)
        {
            _accountService = accountService;
            _requestExecutor = requestExecutor;
            _accessCachingService = accessCachingService;
            _userDataCachingService = userDataCachingService;
            _authorizationService = authorizationService;
        }

        public async Task<ServiceObjectResult<ICollection<Transaction>>> GetAllTransactions(string userId)
        {
            var cachedTransactions = await _userDataCachingService.RetrieveTransactions(userId);
            
            if(!cachedTransactions.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, cachedTransactions.Errors);
            
            if(cachedTransactions.Result != null && cachedTransactions.Result.Any())
                return ServiceObjectResult<ICollection<Transaction>>.Succeeded(cachedTransactions.Result);

            var accessRefreshResult = await _authorizationService.AttemptRefreshAccountContexts(userId);
            
            if(!accessRefreshResult.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, accessRefreshResult.Errors);
            
            var accessContextResult = await _accessCachingService.RetrieveAccessContexts(userId, new List<Scope>{ Scope.transactions, Scope.accounts });

            if (!accessContextResult.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, accessContextResult.Errors);

            if (accessContextResult.Result == null)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, new List<string>{ ErrorMessages.UserNotFound });

            var transactions = new List<Transaction>();

            foreach (var accessContext in accessContextResult.Result)
            {
                var accountLookupResult = await _accountService.GetAccountsInContext(accessContext);

                if (!accountLookupResult.Success)
                    return ServiceObjectResult<ICollection<Transaction>>.Failed(null, accountLookupResult.Errors);

                foreach (var accountId in accountLookupResult.Result.Select(x => x.AccountId).Distinct())
                {
                    var transactionLookupResult =
                        await _requestExecutor.GetTransactions(accessContext.AccessToken, accountId);

                    if (!transactionLookupResult.Success)
                        return ServiceObjectResult<ICollection<Transaction>>.Failed(null,
                            transactionLookupResult.Errors);

                    transactions.AddRange(transactionLookupResult.Result.Results);
                }
            }

            await _userDataCachingService.CacheTransactions(userId, transactions, TimeSpan.FromMinutes(10));

            return ServiceObjectResult<ICollection<Transaction>>.Succeeded(transactions);
        }
        
        
        
    }
}