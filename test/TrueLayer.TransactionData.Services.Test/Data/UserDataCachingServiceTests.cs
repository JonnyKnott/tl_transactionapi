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
    public class UserDataCachingServiceTests
    {
        private Mock<IDistributedCache> _mockCache;
        private readonly Dictionary<string, List<Transaction>> _mockCacheItems;
        private readonly UserDataCachingService _userDataCachingService;
        
        public UserDataCachingServiceTests()
        {
            _mockCache = new Mock<IDistributedCache>();
            
            _userDataCachingService = new UserDataCachingService(_mockCache.Object);
            
            _mockCacheItems = new Dictionary<string, List<Transaction>>();
            SetupRemoveAsync();
        }

        [Fact]
        public async void ClearUserData_Should_Clear_Data_For_Correct_User_Only()
        {
            var user1Data = new List<Transaction> {new Transaction()};
            var user2Data = new List<Transaction> {new Transaction()};
            
            _mockCacheItems.Add(GetKey("user1"), user1Data);
            _mockCacheItems.Add(GetKey("user2"), user2Data);
            
            SetupCacheGetMethod("user1");
            SetupCacheGetMethod("user2");
            SetupCacheSetMethod("user1");
            SetupCacheSetMethod("user2");

            var result = await _userDataCachingService.ClearUserData("user1");
            
            Assert.True(result.Success);
            Assert.Single(_mockCacheItems);
            Assert.Equal(GetKey("user2"), _mockCacheItems.Single().Key);

        }

        [Fact]
        public async void CacheTransactions_Should_Create_New_Cache_Item_If_Not_Exists()
        {
            var user1Data = new List<Transaction> { new Transaction() };
            
            SetupCacheGetMethod("user1");
            SetupCacheSetMethod("user1");

            var result = await _userDataCachingService.CacheTransactions("user1", user1Data, TimeSpan.FromMinutes(10));
            
            Assert.True(result.Success);
            Assert.Single(_mockCacheItems);
            Assert.Equal(GetKey("user1"), _mockCacheItems.Single().Key);
        }

        [Fact]
        public async void RetrieveTransactions_Should_Fetch_Transactions_If_Exist()
        {
            var user1Data = new List<Transaction> { new Transaction() };
            
            _mockCacheItems.Add(GetKey("user1"), user1Data);
            
            SetupCacheGetMethod("user1");

            var result = await _userDataCachingService.RetrieveTransactions("user1");
            
            Assert.True(result.Success);
            Assert.NotNull(result.Result);
            Assert.Single(result.Result);
        }

        private string GetKey(string userId)
        {
            return $"{CacheKeys.TransactionCollectionKeyPrefix}{userId}";
        }
        
        private void SetupCacheGetMethod(string userId)
        {
            _mockCache.Setup(x => x.GetAsync(GetKey(userId), default))
                .ReturnsAsync(_mockCacheItems.ContainsKey(GetKey(userId)) ? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_mockCacheItems[GetKey(userId)])) : default);
        }
        
        private void SetupCacheSetMethod(string userId)
        {
            _mockCache.Setup(x => x.SetAsync(GetKey(userId), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, item, options, cancellation) =>
                {
                    var items =
                        JsonConvert.DeserializeObject<List<Transaction>>(Encoding.UTF8.GetString(item));
                    if(_mockCacheItems.ContainsKey(key)) _mockCacheItems[key] = items;
                    else _mockCacheItems.Add(key, items);
                });
        }

        private void SetupRemoveAsync()
        {
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>(), default))
                .Callback<string, CancellationToken>((key, token) =>
                {
                    if (_mockCacheItems.ContainsKey(key)) _mockCacheItems.Remove(key);
                });
        }
    }
}