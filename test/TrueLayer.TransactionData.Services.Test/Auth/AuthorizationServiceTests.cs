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
        }

        [Fact]
        public async void ExchangeCode_Should_Trigger_ExchangeCode_Request()
        {
            var authResponse = new AuthResponse
            {
                AccessToken = Guid.NewGuid().ToString(),
                ExpiresIn = 10,
                RefreshToken = Guid.NewGuid().ToString()
            };

            _mockRequestExecutor
                .Setup(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<AuthResponse>.Succeeded(authResponse));

            var result = _authorizationService.ExchangeCode(new AccountAccessContext());
            
            _mockRequestExecutor.Verify(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()));
        }
        
        [Fact]
        public async void ExchangeCode_Should_Update_Item_In_Cache_With_New_AccessCode_And_RefreshCode()
        {
            var accessCode = Guid.NewGuid().ToString();
            var displayName = "MyDisplayName";
            var refreshToken = Guid.NewGuid().ToString();
            
            var authResponse = new AuthResponse
            {
                AccessToken = accessCode,
                ExpiresIn = 10,
                RefreshToken = refreshToken
            };

            _mockRequestExecutor
                .Setup(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<AuthResponse>.Succeeded(authResponse));
            
            _mockRequestExecutor
                .Setup(x => x.GetAccessTokenMetadata(accessCode))
                .ReturnsAsync(ServiceObjectResult<TrueLayerListResponse<AccessTokenMetadata>>.Succeeded(new TrueLayerListResponse<AccessTokenMetadata>
                {
                    Results = new List<AccessTokenMetadata>
                    {
                        new AccessTokenMetadata
                        {
                            Provider = new Provider{ DisplayName = displayName }
                        }
                    }
                }));

            _mockAccessContextCachingService.Setup(x => x.PutAccessContext(It.IsAny<AccountAccessContext>())).ReturnsAsync(ServiceResult.Succeeded());

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
            var authResponse = new AuthResponse
            {
                AccessToken = Guid.NewGuid().ToString(),
                ExpiresIn = 10,
                RefreshToken = Guid.NewGuid().ToString()
            };

            _mockRequestExecutor
                .Setup(x => x.ExchangeCode(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<AuthResponse>.Succeeded(authResponse));

            _mockRequestExecutor
                .Setup(x => x.GetAccessTokenMetadata(It.IsAny<string>()))
                .ReturnsAsync(ServiceObjectResult<TrueLayerListResponse<AccessTokenMetadata>>.Succeeded(
                    new TrueLayerListResponse<AccessTokenMetadata>
                    {
                        Results = new List<AccessTokenMetadata>
                        {
                            new AccessTokenMetadata
                            {
                                Provider = new Provider
                                {
                                    DisplayName = "display name"
                                }
                            }
                        }
                    }));

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
    }
}