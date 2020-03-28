using System.Collections.Generic;
using System.Collections.Immutable;
using TrueLayer.TransactionData.Models.Configurations;
using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Services.Data
{
    public class RequestTypeEndpointService : IRequestTypeEndpointService
    {
        private readonly TrueLayerEndpointConfiguration _trueLayerRequestConfiguration;

        public RequestTypeEndpointService(TrueLayerEndpointConfiguration trueLayerRequestConfiguration)
        {
            _trueLayerRequestConfiguration = trueLayerRequestConfiguration;
        }

        public IImmutableDictionary<RequestType, string> RequestEndpoints => new Dictionary<RequestType, string>
        {
            { RequestType.GetAccounts,  _trueLayerRequestConfiguration.GetAccountsEndpoint},
            { RequestType.GetTransactions, _trueLayerRequestConfiguration.GetTransactionsEndpoint},
            { RequestType.ExchangeCode, _trueLayerRequestConfiguration.AuthEndpoint},
            { RequestType.RefreshAccess, _trueLayerRequestConfiguration.AuthEndpoint},
            { RequestType.AccessTokenMetadata, _trueLayerRequestConfiguration.AccessTokenMetadataEndpoint}
        }.ToImmutableDictionary();

        public string GetEndpoint(RequestType requestType)
        {
            return RequestEndpoints[requestType];
        }
    }
}