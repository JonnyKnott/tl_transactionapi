using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAccountContextCachingService _cachingService;
        private readonly IAuthorizationService _authorizationService;

        public AccountService(ITrueLayerDataRequestExecutor requestExecutor, IAccountContextCachingService cachingService, IAuthorizationService authorizationService)
        {
            _requestExecutor = requestExecutor;
            _cachingService = cachingService;
            _authorizationService = authorizationService;
        }

        public async Task<ServiceObjectResult<ICollection<Account>>> GetAccountsInCurrentContext()
        {
            var accountContextsResult = await _cachingService.RetrieveActiveAccountsFromCache();
            
            if(!accountContextsResult.Success)
                return ServiceObjectResult<ICollection<Account>>.Failed(null, accountContextsResult.Errors);
            
            if(!accountContextsResult.Result.Any())
                return ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>());

            var accounts = new List<Account>();
            
            

            foreach (var accountContext in accountContextsResult.Result)
            {
                if (accountContext.AccessToken == null)
                {
                    var accountAccessTokenResult = await _authorizationService.ExchangeCode(accountContext);
                    
                    if(!accountAccessTokenResult.Success)
                        return ServiceObjectResult<ICollection<Account>>.Failed(null, accountAccessTokenResult.Errors);

                    accountContext.AccessToken = accountAccessTokenResult.Result.AccessToken;
                }
                
                if (accountContext.AccessTokenExpiry <= DateTime.UtcNow)
                {
                    var accountContextRefreshResult = await _authorizationService.RefreshAccountContext(accountContext);
                    
                    if(!accountContextRefreshResult.Success)
                        return ServiceObjectResult<ICollection<Account>>.Failed(null, accountContextRefreshResult.Errors);

                    accountContext.AccessToken = accountContextRefreshResult.Result.AccessToken;
                }
                
                var accountsInContextResult = await _requestExecutor.GetAccounts(accountContext.AccessToken);
                
                if(!accountContextsResult.Success)
                    return ServiceObjectResult<ICollection<Account>>.Failed(null, accountContextsResult.Errors);
                
                accounts.AddRange(accountsInContextResult.Result);
            }
            
            return ServiceObjectResult<ICollection<Account>>.Succeeded(accounts);
        }
    }
}