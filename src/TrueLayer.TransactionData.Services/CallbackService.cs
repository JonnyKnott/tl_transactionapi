using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.Services
{
    public class CallbackService : ICallbackService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserDataCachingService _userDataCachingService;

        public CallbackService(IAuthorizationService authorizationService, IUserDataCachingService userDataCachingService)
        {
            _authorizationService = authorizationService;
            _userDataCachingService = userDataCachingService;
        }
        
        public async Task<ServiceResult> Process(string userId, CallbackRequest callbackData)
        {
            if (!string.IsNullOrEmpty(callbackData.Error))
                return ServiceResult.Failed(new[] {ErrorMessages.CallbackStatedAccessDenied});

            var accountContext = new AccountAccessContext
            {
                UserId = userId,
                Code = callbackData.Code,
                Scopes = callbackData.GetScopes()
            };

            var exchangeCodeResult = await _authorizationService.ExchangeCode(accountContext);
            
            if(!exchangeCodeResult.Success)
                return ServiceResult.Failed(exchangeCodeResult.Errors);
            
            await _userDataCachingService.ClearUserData(userId);

            return ServiceResult.Succeeded();
        }
    }
}