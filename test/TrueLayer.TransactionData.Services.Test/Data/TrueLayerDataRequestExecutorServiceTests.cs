using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Configurations;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test.Data
{
    public class TrueLayerDataRequestExecutorServiceTests
    {
        private readonly TrueLayerDataRequestExecutor _trueLayerDataRequestExecutor;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockMessageHandler;

        private const string Accounts = "Accounts";
        private const string ExchangeCode = "ExchangeCode";
        private const string Transactions = "Transactions{accountId}";
        private const string Metadata = "Metadata";
        private const string Refresh = "Refresh";
        private const string BaseUri = "http://test.com/";

        public TrueLayerDataRequestExecutorServiceTests()
        {
            var requestTypeEndpointService = new Mock<IRequestTypeEndpointService>();

            requestTypeEndpointService.Setup(x => x.GetEndpoint(RequestType.GetAccounts)).Returns(Accounts);
            requestTypeEndpointService.Setup(x => x.GetEndpoint(RequestType.GetTransactions)).Returns(Transactions);
            requestTypeEndpointService.Setup(x => x.GetEndpoint(RequestType.AccessTokenMetadata)).Returns(Metadata);
            requestTypeEndpointService.Setup(x => x.GetEndpoint(RequestType.ExchangeCode)).Returns(ExchangeCode);
            requestTypeEndpointService.Setup(x => x.GetEndpoint(RequestType.RefreshAccess)).Returns(Refresh);
            
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            
            _trueLayerDataRequestExecutor = new TrueLayerDataRequestExecutor(_mockHttpClientFactory.Object, requestTypeEndpointService.Object, new TrueLayerRequestConfiguration
            {
                AccountIdPlaceholder = "{accountId}",
                AuthApiUri = "",
                DataApiUri = "",
                ClientId = "abc",
                ClientSecret = "def"
            });
            
            _mockMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(_mockMessageHandler.Object) {BaseAddress = new Uri(BaseUri)});
        }

        [Fact]
        public async void Service_Should_Call_Accounts_Endpoint_For_GetAccounts_Call()
        {
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(Accounts);
            SetupMessageHandler(new TrueLayerListResponse<Account>(), endpoint, HttpStatusCode.OK);
            
            var result = await _trueLayerDataRequestExecutor.GetAccounts(bearer);
            
            Assert.True(result.Success);
            VerifyClientInvocation(endpoint, Times.Once(), bearer);
        }
        
        [Fact]
        public async void Service_Should_Process_Response_Content()
        {
            var returnObject = new TrueLayerListResponse<Account>
            {
                Results = new List<Account>
                {
                    new Account
                    {
                        AccountId = "MyAccountId"
                    }
                }
            };
            
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(Accounts);
            SetupMessageHandler(returnObject, endpoint, HttpStatusCode.OK);
            
            var result = await _trueLayerDataRequestExecutor.GetAccounts(bearer);
            
            Assert.True(result.Success);
            VerifyClientInvocation(endpoint, Times.Once(), bearer);
            Assert.Single(result.Result.Results);
            Assert.Equal("MyAccountId", result.Result.Results.Single().AccountId);
        }
        
        [Fact]
        public async void Service_Should_Call_Metadata_Endpoint_For_Metadata_Call()
        {
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(Metadata);
            SetupMessageHandler(new TrueLayerListResponse<AccessTokenMetadata>(), endpoint, HttpStatusCode.OK);
            
            var result = await _trueLayerDataRequestExecutor.GetAccessTokenMetadata(bearer);
            
            Assert.True(result.Success);
            VerifyClientInvocation(endpoint, Times.Once(), bearer);
        }
        
        [Fact]
        public async void Service_Should_Call_Refresh_Endpoint_For_Refresh_Call()
        {
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(Refresh);
            SetupMessageHandler(new AuthResponse(){ AccessToken = bearer }, endpoint, HttpStatusCode.OK);
            
            var result = await _trueLayerDataRequestExecutor.RefreshAccountAccess(new AccountAccessContext{ AccessToken = bearer});
            
            Assert.True(result.Success);
            VerifyClientInvocation(endpoint, Times.Once());
        }
        
        [Fact]
        public async void Service_Should_Return_Failed_Response_For_Failed_Calls()
        {
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(Accounts);
            SetupMessageHandler(new TrueLayerListResponse<Account>(), endpoint, HttpStatusCode.Unauthorized);
            
            var result = await _trueLayerDataRequestExecutor.GetAccounts(bearer);
            
            Assert.False(result.Success);
            VerifyClientInvocation(endpoint, Times.Once(), bearer);
        }
        
        [Fact]
        public async void Service_Should_Call_ExchangeCode_Endpoint_For_ExchangeCode_Call()
        {
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(ExchangeCode);
            SetupMessageHandler(new AuthResponse(), endpoint, HttpStatusCode.OK);
            
            var result = await _trueLayerDataRequestExecutor.ExchangeCode(new AccountAccessContext{ AccessToken = bearer});
            
            Assert.True(result.Success);
            VerifyClientInvocation(endpoint, Times.Once());
        }
        
        [Fact]
        public async void Service_Should_Inject_AccountId_Into_Transactions_Endpoint_For_GetAccounts_Call()
        {
            var bearer = "myaccesstoken";
            var endpoint = BuildEndpoint(Transactions);
            var injectedEndpoint = endpoint.Replace("{accountId}", "123");
            SetupMessageHandler(new TrueLayerListResponse<Transaction>(), injectedEndpoint, HttpStatusCode.OK);
            
            var result = await _trueLayerDataRequestExecutor.GetTransactions(bearer, "123");
            
            Assert.True(result.Success);
            VerifyClientInvocation(injectedEndpoint, Times.Once(), bearer);
        }
        

        private void VerifyClientInvocation(string endpoint, Times times, string bearerValue = null)
        {
            _mockMessageHandler.Protected()
                .Verify("SendAsync",
                    times, false, ItExpr.Is<HttpRequestMessage>(req => 
                        (req.RequestUri.ToString() == endpoint)
                    && (bearerValue == null || req.Headers.Authorization.ToString() == $"Bearer {bearerValue}")
                    ), ItExpr.IsAny<CancellationToken>());
        }

        private string BuildEndpoint(string endpoint)
        {
            return $"{BaseUri}{endpoint}";
        }

        private void SetupMessageHandler(object returnObject, string endpoint, HttpStatusCode statusCode)
        {
            _mockMessageHandler
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == endpoint),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JsonConvert.SerializeObject(returnObject)),
                })
                .Verifiable();
        }
        
        
    }
}