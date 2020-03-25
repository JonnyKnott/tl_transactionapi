using System;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.Services.Auth
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ITrueLayerDataRequestExecutor _trueLayerDataRequestExecutor;
        private readonly IAccountContextCachingService _accountContextCachingService;

        public AuthorizationService(ITrueLayerDataRequestExecutor trueLayerDataRequestExecutor, IAccountContextCachingService accountContextCachingService)
        {
            _trueLayerDataRequestExecutor = trueLayerDataRequestExecutor;
            _accountContextCachingService = accountContextCachingService;
        }
        public async Task<ServiceObjectResult<AccountAccessContext>> RefreshAccountContext(AccountAccessContext accountContext)
        {
            var authResponseResult = await _trueLayerDataRequestExecutor.RefreshAccountAccess(accountContext);
            
            if(!authResponseResult.Success)
                return ServiceObjectResult<AccountAccessContext>.Failed(null, authResponseResult.Errors);
            
            accountContext.AccessToken = authResponseResult.Result.AccessToken;
            accountContext.AccessTokenExpiry =
                DateTime.UtcNow.AddSeconds(authResponseResult.Result.ExpiresIn);

            var updateCacheResult = await _accountContextCachingService.UpdateAccountContext(accountContext);
            
            if(!updateCacheResult.Success)
                return ServiceObjectResult<AccountAccessContext>.Failed(null, updateCacheResult.Errors);
            
            return ServiceObjectResult<AccountAccessContext>.Succeeded(accountContext);

        }

        public async Task<ServiceObjectResult<AccountAccessContext>> ExchangeCode(AccountAccessContext accountContext)
        {
            var exchangeCodeResult = await _trueLayerDataRequestExecutor.ExchangeCode(accountContext);
            
            if(!exchangeCodeResult.Success)
                return ServiceObjectResult<AccountAccessContext>.Failed(accountContext, exchangeCodeResult.Errors);

            accountContext.AccessToken = exchangeCodeResult.Result.AccessToken;
            accountContext.RefreshToken = exchangeCodeResult.Result.RefreshToken;
            accountContext.AccessTokenExpiry = DateTime.UtcNow.AddSeconds(exchangeCodeResult.Result.ExpiresIn);
            
            return ServiceObjectResult<AccountAccessContext>.Succeeded(accountContext);
        }
    }
}