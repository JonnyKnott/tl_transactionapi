using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test.Data
{
    public class AccessContextCachingServiceTests
    {
        private readonly AccessContextCachingService _accessContextCachingService;

        private readonly Mock<IDistributedCache> _mockCache;

        private readonly Dictionary<string, List<AccountAccessContext>> _mockCacheItems;

        public AccessContextCachingServiceTests()
        {
            _mockCache = new Mock<IDistributedCache>();
            _accessContextCachingService = new AccessContextCachingService(_mockCache.Object);
            
            _mockCacheItems = new Dictionary<string, List<AccountAccessContext>>();
        }

        [Fact]
        public async void PutAccessContext_Should_Update_If_UserId_CredentialsId_ProviderName_Have_Match()
        {
            string userId = Guid.NewGuid().ToString();
            string credentialsId = Guid.NewGuid().ToString();
            string providerName = Guid.NewGuid().ToString();
            string newValue = "MyNewValue";

            _mockCacheItems.Add(userId, new List<AccountAccessContext>{ new AccountAccessContext{ UserId = userId, CredentialsId = credentialsId, ProviderName = providerName} });
            
            SetupCacheGetMethod(userId);
            
            var newContext = new AccountAccessContext{ UserId = userId, CredentialsId = credentialsId, ProviderName = providerName, AccessToken = newValue};

            SetupCacheSetMethod(userId);
            
            var result = await _accessContextCachingService.PutAccessContext(newContext);

            Assert.True(result.Success);
            Assert.Single(_mockCacheItems[userId]);
            _mockCache.Verify(x=> x.GetAsync(userId, default), Times.Once);
            _mockCache.Verify(x=> x.SetAsync(userId, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),default), Times.Once);
            Assert.Equal(newValue, _mockCacheItems[userId].Single().AccessToken);
        }

        [Fact]
        public async void PutAccessContext_Should_Add_Additional_Item_If_Different_ProviderName()
        {
            string userId = Guid.NewGuid().ToString();
            string credentialsId = Guid.NewGuid().ToString();
            string providerName = Guid.NewGuid().ToString();

            _mockCacheItems.Add(userId, new List<AccountAccessContext>{ new AccountAccessContext{ UserId = userId, CredentialsId = credentialsId, ProviderName = providerName} });
            
            SetupCacheGetMethod(userId);
            
            var newContext = new AccountAccessContext{ UserId = userId, CredentialsId = credentialsId, ProviderName = "newProvider"};

            SetupCacheSetMethod(userId);
            
            var result = await _accessContextCachingService.PutAccessContext(newContext);

            Assert.True(result.Success);
            Assert.Equal(2, _mockCacheItems[userId].Count);
            _mockCache.Verify(x=> x.GetAsync(userId, default), Times.Once);
            _mockCache.Verify(x=> x.SetAsync(userId, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),default), Times.Once);
        }

        [Fact]
        public async void PutAccessMethod_Should_Create_New_List_If_New_UserId()
        {
            string userId = Guid.NewGuid().ToString();
            
            SetupCacheGetMethod(userId);
            SetupCacheSetMethod(userId);
            
            var newContext = new AccountAccessContext{ UserId = userId, ProviderName = "newProvider"};

            var result = await _accessContextCachingService.PutAccessContext(newContext);
            
            Assert.True(result.Success);
            Assert.NotEmpty(_mockCacheItems[userId]);
            _mockCache.Verify(x=> x.GetAsync(userId, default), Times.Once);
            _mockCache.Verify(x=> x.SetAsync(userId, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),default), Times.Once);
        }

        [Fact]
        public async void RemoveMethod_Should_Remove_Item_If_Matches_UserId_CredentialsId_ProviderName()
        {
            string userId = Guid.NewGuid().ToString();
            string credentialsId = Guid.NewGuid().ToString();
            string providerName = Guid.NewGuid().ToString();

            var accessContext = new AccountAccessContext
                {UserId = userId, CredentialsId = credentialsId, ProviderName = providerName};

            _mockCacheItems.Add(userId, new List<AccountAccessContext>{ accessContext });

            SetupCacheGetMethod(userId);
            SetupCacheSetMethod(userId);
            
            var result = await _accessContextCachingService.RemoveAccessContext(accessContext);
            
            Assert.True(result.Success);
            Assert.Empty(_mockCacheItems[userId]);
            _mockCache.Verify(x=> x.GetAsync(userId, default), Times.Once);
            _mockCache.Verify(x=> x.SetAsync(userId, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),default), Times.Once);
            
        }
        
        [Fact]
        public async void RemoveMethod_Should_Not_Remove_Item_If_Any_UserId_CredentialsId_ProviderName_Differ()
        {
            string userId = Guid.NewGuid().ToString();
            string credentialsId = Guid.NewGuid().ToString();
            string providerName = Guid.NewGuid().ToString();

            var accessContext = new AccountAccessContext
                {UserId = userId, CredentialsId = credentialsId, ProviderName = providerName};

            _mockCacheItems.Add(userId, new List<AccountAccessContext>{ accessContext });

            SetupCacheGetMethod(userId);
            SetupCacheSetMethod(userId);
            
            var newUserId = new AccountAccessContext
            {
                CredentialsId = accessContext.CredentialsId,
                UserId = Guid.NewGuid().ToString(),
                ProviderName = accessContext.ProviderName
            };

            var result1 = await _accessContextCachingService.RemoveAccessContext(newUserId);
            
            var newProvider = new AccountAccessContext
            {
                ProviderName = Guid.NewGuid().ToString(),
                UserId = accessContext.UserId,
                CredentialsId = accessContext.CredentialsId
            };

            var result2 = await _accessContextCachingService.RemoveAccessContext(newProvider);

            Assert.True(result1.Success);
            Assert.True(result2.Success);
            Assert.Single(_mockCacheItems[userId]);
            Assert.Equal(userId, _mockCacheItems[userId].Single().UserId);
            Assert.Equal(providerName, _mockCacheItems[userId].Single().ProviderName);
            _mockCache.Verify(x=> x.GetAsync(userId, default), Times.Exactly(1));
            _mockCache.Verify(x=> x.SetAsync(userId, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),default), Times.Never);
        }

        [Fact]
        public async void RetrieveAccessContexts_Should_Filter_By_Scope()
        {
            var userId = Guid.NewGuid().ToString();
            
            _mockCacheItems.Add(userId, new List<AccountAccessContext>{ new AccountAccessContext{ Scopes = new List<Scope>()}, new AccountAccessContext{ Scopes = new List<Scope>{ Scope.transactions }} });

            SetupCacheGetMethod(userId);
            SetupCacheSetMethod(userId);
            
            var result = await _accessContextCachingService.RetrieveAccessContexts(userId, new List<Scope>{ Scope.transactions });
            
            Assert.True(result.Success);
            Assert.Single(result.Result);
            Assert.Contains(Scope.transactions, result.Result.Single().Scopes);
        }
        
        [Fact]
        public async void RetrieveAccessContexts_Should_Return_All_Access_Contexts()
        {
            var userId = Guid.NewGuid().ToString();
            
            _mockCacheItems.Add(userId, new List<AccountAccessContext>{ new AccountAccessContext(), new AccountAccessContext() });
            
            SetupCacheGetMethod(userId);
            SetupCacheSetMethod(userId);
            
            var result = await _accessContextCachingService.RetrieveAccessContexts(userId);
            
            Assert.True(result.Success);
            Assert.Equal(2, result.Result.Count);
        }

        private void SetupCacheGetMethod(string userId)
        {
            _mockCache.Setup(x => x.GetAsync(userId, default))
                .ReturnsAsync(_mockCacheItems.ContainsKey(userId) ? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_mockCacheItems[userId])) : default);
        }
        
        private void SetupCacheSetMethod(string userId)
        {
            _mockCache.Setup(x => x.SetAsync(userId, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, item, options, cancellation) =>
                {
                    var items =
                        JsonConvert.DeserializeObject<List<AccountAccessContext>>(Encoding.UTF8.GetString(item));
                    if(_mockCacheItems.ContainsKey(key)) _mockCacheItems[key] = items;
                    else _mockCacheItems.Add(key, items);
                });
        }
    }
}