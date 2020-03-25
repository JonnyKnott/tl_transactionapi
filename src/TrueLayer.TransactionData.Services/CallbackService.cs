using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.Services
{
    public class CallbackService : ICallbackService
    {
        private readonly IAccountContextCachingService _accountContextCachingService;
        private readonly IAuthorizationService _authorizationService;

        public CallbackService(IAccountContextCachingService accountContextCachingService, IAuthorizationService authorizationService)
        {
            _accountContextCachingService = accountContextCachingService;
            _authorizationService = authorizationService;
        }
        
        public async Task<ServiceResult> Process(CallbackRequest callbackData)
        {
            if (!string.IsNullOrEmpty(callbackData.Error))
                return ServiceResult.Failed(new[] {ErrorMessages.CallbackStatedAccessDenied});

            var accountContext = new AccountAccessContext
            {
                Code = callbackData.Code,
                Scopes = callbackData.GetScopes()
            };

            var exchangeCodeResult = await _authorizationService.ExchangeCode(accountContext);
            
            var cacheResult =
                _accountContextCachingService.CacheNewAccountContext(callbackData.Code, callbackData.GetScopes());
            
            return ServiceResult.Succeeded();
        }
    }
}