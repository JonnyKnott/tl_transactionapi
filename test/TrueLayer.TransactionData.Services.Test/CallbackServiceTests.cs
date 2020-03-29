using System;
using System.Collections.Generic;
using Moq;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test
{
    public class CallbackServiceTests
    {
        private readonly CallbackService _callbackService;
        private readonly Mock<IAuthorizationService> _mockAuthorizationService;
        private readonly Mock<IUserDataCachingService> _mockUserDataCachingService;
        
        public CallbackServiceTests()
        {
            _mockAuthorizationService = new Mock<IAuthorizationService>();
            _mockUserDataCachingService = new Mock<IUserDataCachingService>();
            
            _callbackService = new CallbackService(_mockAuthorizationService.Object, _mockUserDataCachingService.Object);

            _mockUserDataCachingService.Setup(x => x.ClearUserData(It.IsAny<string>()))
                .ReturnsAsync(ServiceResult.Succeeded);
        }

        [Fact]
        public async void Process_Should_Call_ExchangeCode_With_New_AccessContext()
        {
            var userId = Guid.NewGuid().ToString();
            var code = Guid.NewGuid().ToString();
            var callbackRequest = new CallbackRequest
            {
                Code = code,
                Scope = $"{Scope.accounts.ToString()} {Scope.offline_access.ToString()}"
            };

            _mockAuthorizationService
                .Setup(x => x.ExchangeCode(It.Is<AccountAccessContext>(context => context.Code == code && context.UserId == userId)))
                .ReturnsAsync(ServiceObjectResult<AccountAccessContext>.Succeeded(new AccountAccessContext
                    {Code = code, UserId = userId}));
            
            var result = await _callbackService.Process(userId, callbackRequest);
            
            Assert.True(result.Success);
            _mockAuthorizationService.Verify(x => x.ExchangeCode(It.Is<AccountAccessContext>(context => context.Code == code && context.UserId == userId)), Times.Once);

        }

        [Fact]
        public async void Process_Should_Clear_User_Data_If_ExchangeCode_Succeeds()
        {
            var userId = Guid.NewGuid().ToString();
            var code = Guid.NewGuid().ToString();
            var callbackRequest = new CallbackRequest
            {
                Code = code,
                Scope = $"{Scope.accounts.ToString()} {Scope.offline_access.ToString()}"
            };

            _mockAuthorizationService
                .Setup(x => x.ExchangeCode(It.Is<AccountAccessContext>(context => context.Code == code && context.UserId == userId)))
                .ReturnsAsync(ServiceObjectResult<AccountAccessContext>.Succeeded(new AccountAccessContext
                    {Code = code, UserId = userId}));
            
            var result = await _callbackService.Process(userId, callbackRequest);
            
            Assert.True(result.Success);
            _mockAuthorizationService.Verify(x => x.ExchangeCode(It.Is<AccountAccessContext>(context => context.Code == code && context.UserId == userId)), Times.Once);
            _mockUserDataCachingService.Verify(x => x.ClearUserData(It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async void Process_Should_Not_Clear_User_Data_If_ExchangeCode_Fails()
        {
            var userId = Guid.NewGuid().ToString();
            var code = Guid.NewGuid().ToString();
            var callbackRequest = new CallbackRequest
            {
                Code = code,
                Scope = $"{Scope.accounts.ToString()} {Scope.offline_access.ToString()}"
            };

            _mockAuthorizationService
                .Setup(x => x.ExchangeCode(It.Is<AccountAccessContext>(context => context.Code == code && context.UserId == userId)))
                .ReturnsAsync(ServiceObjectResult<AccountAccessContext>.Failed(null, new List<string>()));
            
            var result = await _callbackService.Process(userId, callbackRequest);
            
            Assert.False(result.Success);
            _mockAuthorizationService.Verify(x => x.ExchangeCode(It.Is<AccountAccessContext>(context => context.Code == code && context.UserId == userId)), Times.Once);
            _mockUserDataCachingService.Verify(x => x.ClearUserData(It.IsAny<string>()), Times.Never);

        }
        
    }
}