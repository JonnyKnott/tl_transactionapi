using System;
using System.Linq;
using TrueLayer.TransactionData.Models.Configurations;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Services.Data;
using Xunit;

namespace TrueLayer.TransactionData.Services.Test.Data
{
    public class RequestTypeEndpointServiceTests
    {
        private readonly RequestTypeEndpointService _requestTypeEndpointService;
        
        public RequestTypeEndpointServiceTests()
        {
            _requestTypeEndpointService = new RequestTypeEndpointService(new TrueLayerEndpointConfiguration
            {
                AuthEndpoint = "auth",
                GetAccountsEndpoint = "accounts",
                GetTransactionsEndpoint = "transactions",
                AccessTokenMetadataEndpoint = "accessToken"
            });
        }

        [Fact]
        public async void Service_Should_Contain_Endpoint_For_Every_RequestType()
        {
            foreach (var requestType in Enum.GetValues(typeof(RequestType)).Cast<RequestType>().ToList())
            {
                Assert.NotNull(_requestTypeEndpointService.GetEndpoint(requestType));
            }
        }
    }
}