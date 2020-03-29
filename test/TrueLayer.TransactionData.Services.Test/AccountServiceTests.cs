using System.Collections.Generic;
using Moq;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test
{
    public class AccountServiceTests
    {
        private readonly AccountService _accountService;
        private readonly Mock<ITrueLayerDataRequestExecutor> _mockTrueLayerRequestExecutor;

        public AccountServiceTests()
        {
            _mockTrueLayerRequestExecutor = new Mock<ITrueLayerDataRequestExecutor>();
            
            _accountService = new AccountService(_mockTrueLayerRequestExecutor.Object);
        }

        [Fact]
        public async void GetAccountsInContext_Should_Call_Executor_Method_With_Context_Token()
        {
            var accessContext = new AccountAccessContext
            {
                AccessToken = "AccessToken"
            };

            _mockTrueLayerRequestExecutor.Setup(x => x.GetAccounts(accessContext.AccessToken))
                .ReturnsAsync(ServiceObjectResult<TrueLayerListResponse<Account>>.Succeeded(
                    new TrueLayerListResponse<Account>
                    {
                        Status = "Succeeded",
                        Results = new List<Account>
                        {
                            new Account()
                        }
                    }));
            
            var result = await _accountService.GetAccountsInContext(accessContext);
            
            Assert.True(result.Success);
            _mockTrueLayerRequestExecutor.Verify(x => x.GetAccounts(accessContext.AccessToken), Times.Once);
        }
        [Fact]
        public async void GetAccountsInContext_Should_Return_Failed_If_Executor_Call_Fails()
        {
            var accessContext = new AccountAccessContext
            {
                AccessToken = "AccessToken"
            };

            _mockTrueLayerRequestExecutor.Setup(x => x.GetAccounts(accessContext.AccessToken))
                .ReturnsAsync(ServiceObjectResult<TrueLayerListResponse<Account>>.Failed(null, new List<string>{ "Value" }));
            
            var result = await _accountService.GetAccountsInContext(accessContext);
            
            Assert.False(result.Success);
            _mockTrueLayerRequestExecutor.Verify(x => x.GetAccounts(accessContext.AccessToken), Times.Once);
        }
    }
}