using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Auth;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test
{
    public class TransactionServiceTests
    {
        private readonly TransactionService _transactionService;

        private readonly Mock<IAccountService> _mockAccountService;
        private readonly Mock<ITrueLayerDataRequestExecutor> _mockTrueLayerDataRequestExecutor;
        private readonly Mock<IAccessContextCachingService> _mockAccessContextCachingService;
        private readonly Mock<IUserDataCachingService> _mockUserDataCachingService;
        private readonly Mock<IAuthorizationService> _mockAuthorizationService;
        
        public TransactionServiceTests()
        {
            _mockAccountService = new Mock<IAccountService>();
            _mockAuthorizationService = new Mock<IAuthorizationService>();
            _mockAccessContextCachingService = new Mock<IAccessContextCachingService>();
            _mockUserDataCachingService = new Mock<IUserDataCachingService>();
            _mockTrueLayerDataRequestExecutor = new Mock<ITrueLayerDataRequestExecutor>();
            
            _transactionService = new TransactionService(
                _mockAccountService.Object, 
                _mockTrueLayerDataRequestExecutor.Object, 
                _mockAccessContextCachingService.Object,
                _mockUserDataCachingService.Object,
                _mockAuthorizationService.Object);
            
            _mockUserDataCachingService.Setup(x => x.RetrieveTransactions(It.IsAny<string>()))
                .ReturnsAsync(ServiceObjectResult<ICollection<Transaction>>.Succeeded(new List<Transaction>()));
        }


        [Fact]
        public async void GetAllTransactions_Should_Return_Cached_Transaction_If_Exist()
        {
            string userId = Guid.NewGuid().ToString();

            _mockUserDataCachingService.Setup(x => x.RetrieveTransactions(userId))
                .ReturnsAsync(ServiceObjectResult<ICollection<Transaction>>.Succeeded(new List<Transaction>(){ new Transaction() }));

            var result = await _transactionService.GetAllTransactions(userId);
            
            Assert.True(result.Success);
            Assert.Single(result.Result);
        }

        [Fact]
        public async void GetAllTransactions_Should_Both_Refresh_Accounts_And_Retrieve_Active_Accounts()
        {
            var userId = Guid.NewGuid().ToString();

            SetupEmptyCacheCall(userId);

            _mockAuthorizationService.Setup(x => x.AttemptRefreshAccountContexts(userId))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccessContextRetrieval(userId, new List<AccountAccessContext>());
            
            var result = await _transactionService.GetTransactionCategoryResponses(userId);

            _mockAuthorizationService.Verify(x => x.AttemptRefreshAccountContexts(userId), Times.Once());
            _mockAccessContextCachingService.Verify(x => x.RetrieveAccessContexts(userId, It.IsAny<ICollection<Scope>>()), Times.Once);

        }
        
        [Fact]
        public async void GetAllTransactions_Should_Retrieve_Accounts_For_Each_Context()
        {
            var userId = Guid.NewGuid().ToString();
            var accessCode1 = Guid.NewGuid().ToString();
            var accessCode2 = Guid.NewGuid().ToString();

            SetupEmptyCacheCall(userId);

            _mockAuthorizationService.Setup(x => x.AttemptRefreshAccountContexts(userId))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccessContextRetrieval(userId, new List<AccountAccessContext>{ new AccountAccessContext{ AccessToken = accessCode1}, new AccountAccessContext{ AccessToken = accessCode2 } });

            _mockAccountService.Setup(x =>
                    x.GetAccountsInContext(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>()));
            
            var result = await _transactionService.GetTransactionCategoryResponses(userId);

            _mockAccountService.Verify(x => x.GetAccountsInContext(It.Is<AccountAccessContext>(
                context => context.AccessToken == accessCode1)), Times.Once);
            _mockAccountService.Verify(x => x.GetAccountsInContext(It.Is<AccountAccessContext>(
                context => context.AccessToken == accessCode2)), Times.Once);
        }
        
        [Fact]
        public async void GetAllTransactions_Should_Retrieve_Transactions_For_Each_Account()
        {
            var userId = Guid.NewGuid().ToString();
            var accessCode1 = Guid.NewGuid().ToString();
            var accessCode2 = Guid.NewGuid().ToString();
            var accountCode1 = Guid.NewGuid().ToString();
            var accountCode2 = Guid.NewGuid().ToString();
            var accountCode3 = Guid.NewGuid().ToString();
            var accountCode4 = Guid.NewGuid().ToString();

            SetupEmptyCacheCall(userId);

            _mockAuthorizationService.Setup(x => x.AttemptRefreshAccountContexts(userId))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccessContextRetrieval(userId, new List<AccountAccessContext>{ new AccountAccessContext{ AccessToken = accessCode1}, new AccountAccessContext{ AccessToken = accessCode2 } });

            _mockAccountService.Setup(x =>
                    x.GetAccountsInContext(It.Is<AccountAccessContext>(context => context.AccessToken == accessCode1)))
                .ReturnsAsync(ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>{ new Account{ AccountId = accountCode1}, new Account{ AccountId = accountCode2} }));
            
            _mockAccountService.Setup(x =>
                    x.GetAccountsInContext(It.Is<AccountAccessContext>(context => context.AccessToken == accessCode2)))
                .ReturnsAsync(ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>{ new Account{ AccountId = accountCode3}, new Account{ AccountId = accountCode4} }));

            _mockTrueLayerDataRequestExecutor.Setup(x => x.GetTransactions(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(
                    ServiceObjectResult<TrueLayerListResponse<Transaction>>.Succeeded(
                        new TrueLayerListResponse<Transaction>{ Results = new List<Transaction>()}));
            
            var result = await _transactionService.GetTransactionCategoryResponses(userId);
            
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode1, accountCode1), Times.Once);
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode1, accountCode2), Times.Once);
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode2, accountCode3), Times.Once);
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode2, accountCode4), Times.Once);
        }
        
        [Fact]
        public async void GetTransactionCategoryResponses_Should_Both_Refresh_Accounts_And_Retrieve_Active_Accounts()
        {
            var userId = Guid.NewGuid().ToString();

            SetupEmptyCacheCall(userId);

            _mockAuthorizationService.Setup(x => x.AttemptRefreshAccountContexts(userId))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccessContextRetrieval(userId, new List<AccountAccessContext>());
            
            var result = await _transactionService.GetTransactionCategoryResponses(userId);

            _mockAuthorizationService.Verify(x => x.AttemptRefreshAccountContexts(userId), Times.Once());
            _mockAccessContextCachingService.Verify(x => x.RetrieveAccessContexts(userId, It.IsAny<ICollection<Scope>>()), Times.Once);

        }
        
        [Fact]
        public async void GetTransactionCategoryResponses_Should_Retrieve_Accounts_For_Each_Context()
        {
            var userId = Guid.NewGuid().ToString();
            var accessCode1 = Guid.NewGuid().ToString();
            var accessCode2 = Guid.NewGuid().ToString();

            SetupEmptyCacheCall(userId);

            _mockAuthorizationService.Setup(x => x.AttemptRefreshAccountContexts(userId))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccessContextRetrieval(userId, new List<AccountAccessContext>{ new AccountAccessContext{ AccessToken = accessCode1}, new AccountAccessContext{ AccessToken = accessCode2 } });

            _mockAccountService.Setup(x =>
                    x.GetAccountsInContext(It.IsAny<AccountAccessContext>()))
                .ReturnsAsync(ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>()));
            
            var result = await _transactionService.GetTransactionCategoryResponses(userId);

            _mockAccountService.Verify(x => x.GetAccountsInContext(It.Is<AccountAccessContext>(
                context => context.AccessToken == accessCode1)), Times.Once);
            _mockAccountService.Verify(x => x.GetAccountsInContext(It.Is<AccountAccessContext>(
                context => context.AccessToken == accessCode2)), Times.Once);
        }
        
        [Fact]
        public async void GetTransactionCategoryResponses_Should_Retrieve_Transactions_For_Each_Account()
        {
            var userId = Guid.NewGuid().ToString();
            var accessCode1 = Guid.NewGuid().ToString();
            var accessCode2 = Guid.NewGuid().ToString();
            var accountCode1 = Guid.NewGuid().ToString();
            var accountCode2 = Guid.NewGuid().ToString();
            var accountCode3 = Guid.NewGuid().ToString();
            var accountCode4 = Guid.NewGuid().ToString();

            SetupEmptyCacheCall(userId);

            _mockAuthorizationService.Setup(x => x.AttemptRefreshAccountContexts(userId))
                .ReturnsAsync(ServiceResult.Succeeded);

            SetupAccessContextRetrieval(userId, new List<AccountAccessContext>{ new AccountAccessContext{ AccessToken = accessCode1}, new AccountAccessContext{ AccessToken = accessCode2 } });

            _mockAccountService.Setup(x =>
                    x.GetAccountsInContext(It.Is<AccountAccessContext>(context => context.AccessToken == accessCode1)))
                .ReturnsAsync(ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>{ new Account{ AccountId = accountCode1}, new Account{ AccountId = accountCode2} }));
            
            _mockAccountService.Setup(x =>
                    x.GetAccountsInContext(It.Is<AccountAccessContext>(context => context.AccessToken == accessCode2)))
                .ReturnsAsync(ServiceObjectResult<ICollection<Account>>.Succeeded(new List<Account>{ new Account{ AccountId = accountCode3}, new Account{ AccountId = accountCode4} }));

            _mockTrueLayerDataRequestExecutor.Setup(x => x.GetTransactions(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(
                    ServiceObjectResult<TrueLayerListResponse<Transaction>>.Succeeded(
                        new TrueLayerListResponse<Transaction>{ Results = new List<Transaction>()}));
            
            var result = await _transactionService.GetTransactionCategoryResponses(userId);
            
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode1, accountCode1), Times.Once);
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode1, accountCode2), Times.Once);
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode2, accountCode3), Times.Once);
            _mockTrueLayerDataRequestExecutor.Verify(x => x.GetTransactions(accessCode2, accountCode4), Times.Once);
        }

        [Fact]
        public async void GetTransactionCategoryResponse_Should_Correctly_Group_Transactions()
        {
            string userId = Guid.NewGuid().ToString();

            _mockUserDataCachingService.Setup(x => x.RetrieveTransactions(userId))
                .ReturnsAsync(ServiceObjectResult<ICollection<Transaction>>.Succeeded(BuildTestTransactionData()));

            var result = await _transactionService.GetTransactionCategoryResponses(userId);
            
            Assert.True(result.Success);
            Assert.Equal(2, result.Result.CreditTransactions.Count);
            Assert.Equal(2, result.Result.DebitTransactions.Count);
            Assert.Equal(20.00, result.Result.DebitTransactions.First().AverageAmount);
            Assert.Equal(20.00, result.Result.DebitTransactions.Last().AverageAmount);
            Assert.Equal(40.00, result.Result.DebitTransactions.First().TotalAmount);
            Assert.Equal(40.00, result.Result.DebitTransactions.Last().TotalAmount);
        }
        
        [Fact]
        public async void GetTransactionCategoryResponse_Should_Correctly_Group_Transactions_With_Detailed()
        {
            string userId = Guid.NewGuid().ToString();

            _mockUserDataCachingService.Setup(x => x.RetrieveTransactions(userId))
                .ReturnsAsync(ServiceObjectResult<ICollection<Transaction>>.Succeeded(BuildTestTransactionData()));

            var result = await _transactionService.GetTransactionCategoryResponses(userId, true);
            
            Assert.True(result.Success);
            Assert.Equal(4, result.Result.CreditTransactions.Count);
            Assert.Equal(4, result.Result.DebitTransactions.Count);
            Assert.Equal(20.00, result.Result.DebitTransactions.First().AverageAmount);
            Assert.Equal(20.00, result.Result.DebitTransactions.Last().AverageAmount);
            Assert.Equal(20.00, result.Result.DebitTransactions.First().TotalAmount);
            Assert.Equal(20.00, result.Result.DebitTransactions.Last().TotalAmount);
        }

        private void SetupAccessContextRetrieval(string userId, ICollection<AccountAccessContext> accessContexts)
        {
            _mockAccessContextCachingService
                .Setup(x => x.RetrieveAccessContexts(userId, It.IsAny<ICollection<Scope>>()))
                .ReturnsAsync(ServiceObjectResult<ICollection<AccountAccessContext>>.Succeeded(accessContexts));

        }

        private void SetupEmptyCacheCall(string userId)
        {
            _mockUserDataCachingService.Setup(x => x.RetrieveTransactions(userId))
                .ReturnsAsync(ServiceObjectResult<ICollection<Transaction>>.Succeeded(new List<Transaction>()));
        }

        private ICollection<Transaction> BuildTestTransactionData()
        {
            return new List<Transaction>
            {
                new Transaction
                {
                    TransactionType = TransactionType.Credit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory1",
                        "SubCategory1"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Credit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory1",
                        "SubCategory2"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Credit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory2",
                        "SubCategory3"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Credit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory2",
                        "SubCategory4"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Debit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory1",
                        "SubCategory1"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Debit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory1",
                        "SubCategory2"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Debit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory2",
                        "SubCategory3"
                    }
                },
                new Transaction
                {
                    TransactionType = TransactionType.Debit,
                    Amount = 20.0000000,
                    TransactionCategory = TransactionCategory.CHEQUE,
                    TransactionClassifications = new List<string>
                    {
                        "ParentCategory2",
                        "SubCategory4"
                    }
                }
            };
        }
    }
}