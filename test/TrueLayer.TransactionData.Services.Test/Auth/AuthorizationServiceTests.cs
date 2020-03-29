using System;
using System.Collections.Generic;
using Moq;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test.Auth
{
    public class AuthorizationServiceTests
    {
        private readonly AuthorizationService _authorizationService;

        private readonly Mock<ITrueLayerDataRequestExecutor> _mockRequestExecutor;
        private readonly Mock<IAccessContextCachingService> _mockAccessContextCachingService;

        public AuthorizationServiceTests()
        {
            _mockRequestExecutor = new Mock<ITrueLayerDataRequestExecutor>();
            _mockAccessContextCachingService = new Mock<IAccessContextCachingService>();
            
            _authorizationService = new AuthorizationService(_mockRequestExecutor.Object, _mockAccessContextCachingService.Object);
            
            _mockAccessContextCachingService.Setup(x => x.RemoveAccessContext(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceResult.Succeeded);
            _mockAccessContextCachingService.Setup(x => x.PutAccessContext(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccountContexts(false, true);

        }

        [Fact]
        public async void ExchangeCode_Should_Trigger_ExchangeCode_Request()
        {
            SetupExchangeCodeRequest();

            var result = _authorizationService.ExchangeCode(new AccountAccessContext());
            
            _mockRequestExecutor.Verify(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()));
        }
        
        [Fact]
        public async void ExchangeCode_Should_Update_Item_In_Cache_With_New_AccessCode_And_RefreshCode()
        {
            var accessCode = Guid.NewGuid().ToString();
            var displayName = "MyDisplayName";
            var refreshToken = Guid.NewGuid().ToString();
            
            SetupExchangeCodeRequest(true, accessCode, refreshToken);
            SetupAccessTokenMetadataCall();
            
            
            var result = await _authorizationService.ExchangeCode(new AccountAccessContext());
            
            _mockAccessContextCachingService.Verify(x => x.PutAccessContext(
                It.Is<AccountAccessContext>((o, type) => 
                    (o as AccountAccessContext).AccessToken == accessCode && 
                    (o as AccountAccessContext).RefreshToken == refreshToken)
                ));
        }
        
        [Fact]
        public async void ExchangeCode_Should_Trigger_AccessCodeMetadata_Request()
        {
            SetupExchangeCodeRequest();
            SetupAccessTokenMetadataCall();

            var result = await _authorizationService.ExchangeCode(new AccountAccessContext());
            
            _mockRequestExecutor.Verify(x => x.GetAccessTokenMetadata(It.IsAny<string>()));
        }

        [Fact]
        public async void ExchangeCode_Should_Return_Failed_If_Call_Fails()
        {
            var myError = "Error";
            
            _mockRequestExecutor
                .Setup(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<AuthResponse>.Failed(null, new List<string>{ myError }));
            
            var result = await _authorizationService.ExchangeCode(new AccountAccessContext());
            
            Assert.False(result.Success);
            Assert.Contains(myError, result.Errors);
        }

        [Fact]
        public async void RefreshAccountTokens_Should_Not_Act_If_No_Refresh_Is_Necessary()
        {
            SetupAccountContexts(false, true);

            var result = await _authorizationService.AttemptRefreshAccountContexts("myuserid");
            
            Assert.True(result.Success);
            Assert.Empty(_mockRequestExecutor.Invocations);
        }

        [Fact]
        public async void RefreshAccountTokens_Should_Refresh_If_Required()
        {
            SetupAccountContexts(true, true);
            SetupRefreshCodeCall();

            var result = await _authorizationService.AttemptRefreshAccountContexts("myuser");
            
            Assert.True(result.Success);
            
            _mockRequestExecutor.Verify(x => x.RefreshAccountAccess(It.IsAny<AccountAccessContext>()), Times.Once);
        }

        [Fact]
        public async void Service_Fails_If_AccessTokenMetadata_Cannot_Be_Obtained()
        {
            SetupAccountContexts(true, true);
            SetupExchangeCodeRequest();

            _mockRequestExecutor.Setup(x => x.GetAccessTokenMetadata(It.IsAny<string>()))
                .ReturnsAsync(
                    ServiceObjectResult<TrueLayerListResponse<AccessTokenMetadata>>.Succeeded(null));

            var result = await _authorizationService.ExchangeCode(new AccountAccessContext());
            
            Assert.False(result.Success);
            Assert.Contains(ErrorMessages.FailedToObtainAccessTokenMetadata, result.Errors);

        }
        
        [Fact]
        public async void RefreshAccountTokens_Should_Remove_Access_If_RefreshToken_Empty_When_Expired()
        {
            _mockAccessContextCachingService.Setup(x => x.RetrieveAccessContexts(It.IsAny<string>(), null))
                .ReturnsAsync(ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(new List<AccountAccessContext>
                {
                    new AccountAccessContext
                    {
                        AccessTokenExpiry = DateTime.UtcNow
                    }
                }));

            _mockRequestExecutor.Setup(x => x.RefreshAccountAccess(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<AuthResponse>.Succeeded(new AuthResponse
                {
                    ExpiresIn = 9999,
                    AccessToken = "string",
                    RefreshToken = "string"
                }));

            var result = await _authorizationService.AttemptRefreshAccountContexts("myuser");
            
            Assert.True(result.Success);

            _mockRequestExecutor.Verify(x => x.RefreshAccountAccess(It.IsAny<AccountAccessContext>()), Times.Never());
            _mockAccessContextCachingService.Verify(x => x.RemoveAccessContext(It.IsAny<AccountAccessContext>()), Times.Once);
        }
        
                private void SetupAccountContexts(bool expired, bool hasRefreshToken)
        {
            var accountContext = new AccountAccessContext{ AccessTokenExpiry = DateTime.UtcNow.AddMonths(1)};
            
            if (expired)
                accountContext.AccessTokenExpiry = DateTime.UtcNow;
            
            if (hasRefreshToken)
                accountContext.RefreshToken = "string";
            
            _mockAccessContextCachingService.Setup(x => x.RetrieveAccessContexts(It.IsAny<string>(), null))
                .ReturnsAsync(
                    ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(new List<AccountAccessContext>{ accountContext }));
        }

        private void SetupExchangeCodeRequest(bool succeeded = true, string accessToken = null, string refreshToken = null)
        {
            var authResponse = new AuthResponse
            {
                AccessToken = accessToken ?? Guid.NewGuid().ToString(),
                ExpiresIn = 10,
                RefreshToken = refreshToken ?? Guid.NewGuid().ToString()
            };

            _mockRequestExecutor
                .Setup(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(succeeded ? ServiceObjectResult<AuthResponse>.Succeeded(authResponse) : ServiceObjectResult<AuthResponse>.Failed(null, new List<string>{ "Value" }));
        }

        private void SetupAccessTokenMetadataCall(string displayName = null)
        {
            _mockRequestExecutor
                .Setup(x => x.GetAccessTokenMetadata(It.IsAny<string>()))
                .ReturnsAsync(ServiceObjectResult<TrueLayerListResponse<AccessTokenMetadata>>.Succeeded(new TrueLayerListResponse<AccessTokenMetadata>
                {
                    Results = new List<AccessTokenMetadata>
                    {
                        new AccessTokenMetadata
                        {
                            Provider = new Provider{ DisplayName = displayName ?? "displayName" }
                        }
                    }
                }));
        }
        
        private void SetupRefreshCodeCall()
        {
            _mockRequestExecutor.Setup(x => x.RefreshAccountAccess(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<AuthResponse>.Succeeded(new AuthResponse
                {
                    ExpiresIn = 9999,
                    AccessToken = "string",
                    RefreshToken = "string"
                }));
        }
    }
}