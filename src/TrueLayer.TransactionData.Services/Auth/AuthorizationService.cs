using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data;

namespace TrueLayer.TransactionData.Services.Auth
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ITrueLayerDataRequestExecutor _trueLayerDataRequestExecutor;
        private readonly IAccessContextCachingService _accessContextCachingService;

        public AuthorizationService(ITrueLayerDataRequestExecutor trueLayerDataRequestExecutor, IAccessContextCachingService accessContextCachingService)
        {
            _trueLayerDataRequestExecutor = trueLayerDataRequestExecutor;
            _accessContextCachingService = accessContextCachingService;
        }
        
        public async Task<ServiceResult> AttemptRefreshAccountContexts(string userId)
        {
            var accessContextsForUser = await _accessContextCachingService.RetrieveAccessContexts(userId);
            
            if(!accessContextsForUser.Success)
                return ServiceResult.Failed(accessContextsForUser.Errors);
            
            if(!accessContextsForUser.Result.Any(x => x.IsExpiredOrClose))
                return ServiceResult.Succeeded();

            foreach (var accessContext in accessContextsForUser.Result.Where(x => x.IsExpiredOrClose))
            {
                if (accessContext.RefreshToken == null)
                {
                    await _accessContextCachingService.RemoveAccessContext(accessContext);
                    continue;
                }

                var authResponseResult = await _trueLayerDataRequestExecutor.RefreshAccountAccess(accessContext);

                if (!authResponseResult.Success)
                    return ServiceResult.Failed(authResponseResult.Errors);

                accessContext.AccessToken = authResponseResult.Result.AccessToken;
                accessContext.RefreshToken = authResponseResult.Result.RefreshToken;
                accessContext.AccessTokenExpiry =
                    DateTime.UtcNow.AddSeconds(authResponseResult.Result.ExpiresIn);

                var updateCacheResult = await _accessContextCachingService.PutAccessContext(accessContext);

                if (!updateCacheResult.Success)
                    return ServiceResult.Failed(updateCacheResult.Errors);
            }
            
            return ServiceResult.Succeeded();
        }

        public async Task<ServiceObjectResult<AccountAccessContext>> ExchangeCode(AccountAccessContext accessContext)
        {
            var exchangeCodeResult = await _trueLayerDataRequestExecutor.ExchangeCode(accessContext);
            
            if(!exchangeCodeResult.Success)
                return ServiceObjectResult<AccountAccessContext>.Failed(accessContext, exchangeCodeResult.Errors);

            accessContext.AccessToken = exchangeCodeResult.Result.AccessToken;
            accessContext.RefreshToken = exchangeCodeResult.Result.RefreshToken;
            
            var saveContextResult = await ObtainMetadataAndUpdateCache(accessContext);
            
            if(!saveContextResult.Success)
                return ServiceObjectResult<AccountAccessContext>.Failed(null, saveContextResult.Errors);
            
            return ServiceObjectResult<AccountAccessContext>.Succeeded(accessContext);
        }

        private async Task<ServiceResult> ObtainMetadataAndUpdateCache(AccountAccessContext accessContext)
        {
            var accessMetadataResult =
                await _trueLayerDataRequestExecutor.GetAccessTokenMetadata(accessContext.AccessToken);
            
            if(!accessMetadataResult.Success)
                return ServiceResult.Failed(accessMetadataResult.Errors);

            if(accessMetadataResult.Result == null)
                return ServiceResult.Failed(new List<string>{ ErrorMessages.FailedToObtainAccessTokenMetadata });

            var accessMetadata = accessMetadataResult.Result.Results.Single();
            
            accessContext.AccessTokenExpiry = accessMetadata.ConsentExpiresAt;
            accessContext.ProviderName = accessMetadata.Provider.DisplayName;
            accessContext.CredentialsId = accessMetadata.CredentialsId;

            await _accessContextCachingService.PutAccessContext(accessContext);
            
            return ServiceResult.Succeeded();
        }

        // public async Task<ServiceObjectResult>
    }
}