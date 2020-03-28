using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;
using TrueLayer.TransactionData.Services.Extensions;

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
            //Check cache
            var cachedTransactions = await _userDataCachingService.RetrieveTransactions(userId);
            
            if(!cachedTransactions.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, cachedTransactions.Errors);
            
            if(cachedTransactions.Result != null && cachedTransactions.Result.Any())
                return ServiceObjectResult<ICollection<Transaction>>.Succeeded(cachedTransactions.Result);

            //Obtain all valid access contexts 
            var accessContextResult = await ObtainValidAccountContexts(userId);

            if(!accessContextResult.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, accessContextResult.Errors);

            var transactionsResult = await ObtainTransactionsForAccountContexts(accessContextResult.Result);

            if(!transactionsResult.Success)
                return ServiceObjectResult<ICollection<Transaction>>.Failed(null, transactionsResult.Errors);
            
            await _userDataCachingService.CacheTransactions(userId, transactionsResult.Result, TimeSpan.FromMinutes(10));

            return ServiceObjectResult<ICollection<Transaction>>.Succeeded(transactionsResult.Result);
        }

        public async Task<ServiceObjectResult<TransactionCategoryResponse>>
            GetTransactionCategoryResponses(string userId, bool detailed = false)
        {
            //Check cache
            var cachedTransactions = await _userDataCachingService.RetrieveTransactions(userId);
            
            if(!cachedTransactions.Success)
                return ServiceObjectResult<TransactionCategoryResponse>.Failed(null, cachedTransactions.Errors);
            
            if(cachedTransactions.Result != null && cachedTransactions.Result.Any())
                return ServiceObjectResult<TransactionCategoryResponse>.Succeeded(CreateCategoryResponse(cachedTransactions.Result, detailed));
            
            //Obtain all valid access contexts 
            var accessContextResult = await ObtainValidAccountContexts(userId);

            if(!accessContextResult.Success)
                return ServiceObjectResult<TransactionCategoryResponse>.Failed(null, accessContextResult.Errors);

            var transactionsResult = await ObtainTransactionsForAccountContexts(accessContextResult.Result);

            if(!transactionsResult.Success)
                return ServiceObjectResult<TransactionCategoryResponse>.Failed(null, transactionsResult.Errors);
            
            await _userDataCachingService.CacheTransactions(userId, transactionsResult.Result, TimeSpan.FromMinutes(10));

            return ServiceObjectResult<TransactionCategoryResponse>.Succeeded(CreateCategoryResponse(transactionsResult.Result, detailed));
        }

        private TransactionCategoryResponse CreateCategoryResponse(ICollection<Transaction> transactions, bool detailed)
        {
            var groupedDebitTransactions = transactions.Where(x => x.TransactionType == TransactionType.Debit).GroupBy(x => detailed ? x.TransactionClassifications.LastOrDefault() : x.TransactionClassifications.FirstOrDefault())
                .Select(x => x.CreateCategorySummary()).ToList();
            
            var groupedCreditTransactions = transactions.Where(x => x.TransactionType == TransactionType.Credit).GroupBy(x => detailed ? x.TransactionClassifications.LastOrDefault() : x.TransactionClassifications.FirstOrDefault())
                .Select(x => x.CreateCategorySummary()).ToList();

            return new TransactionCategoryResponse
            {
                DebitTransactions = groupedDebitTransactions,
                CreditTransactions = groupedCreditTransactions
            };
        } 

        private async Task<ServiceObjectResult<ICollection<Transaction>>> ObtainTransactionsForAccountContexts(
            ICollection<AccountAccessContext> accessContexts)
        {
            var transactions = new List<Transaction>();

            foreach (var accessContext in accessContexts)
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

            return ServiceObjectResult<ICollection<Transaction>>.Succeeded(transactions);
        }

        private async Task<ServiceObjectResult<ICollection<AccountAccessContext>>> ObtainValidAccountContexts(string userId)
        {
            //Refresh access on all users accounts where required
            var accessRefreshResult = await _authorizationService.AttemptRefreshAccountContexts(userId);
            
            if(!accessRefreshResult.Success)
                return ServiceObjectResult<ICollection<AccountAccessContext>>.Failed(null, accessRefreshResult.Errors);
            
            //ObtainAccessContexts
            var accessContextResult = await _accessCachingService.RetrieveAccessContexts(userId, new List<Scope>{ Scope.transactions, Scope.accounts });

            if (!accessContextResult.Success)
                return ServiceObjectResult<ICollection<AccountAccessContext>>.Failed(null, accessContextResult.Errors);

            if (accessContextResult.Result == null)
                return ServiceObjectResult<ICollection<AccountAccessContext>>.Failed(null, new List<string>{ ErrorMessages.UserNotFound });

            return ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(accessContextResult.Result);
            
        }
    }
}