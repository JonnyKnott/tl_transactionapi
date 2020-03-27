using System.Collections.Generic;
using System.Collections.Immutable;
using TrueLayer.TransactionData.Models.Configurations;
using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Services.Data
{
    public class RequestTypeEndpointService : IRequestTypeEndpointService
    {
        private readonly TrueLayerConfiguration _trueLayerConfiguration;

        public RequestTypeEndpointService(TrueLayerConfiguration trueLayerConfiguration)
        {
            _trueLayerConfiguration = trueLayerConfiguration;
        }

        public IImmutableDictionary<RequestType, string> RequestEndpoints => new Dictionary<RequestType, string>
        {
            { RequestType.GetAccounts,  _trueLayerConfiguration.GetAccountsEndpoint},
            { RequestType.GetTransactions, _trueLayerConfiguration.GetTransactionsEndpoint},
            { RequestType.ExchangeCode, _trueLayerConfiguration.AuthEndpoint},
            { RequestType.RefreshAccess, _trueLayerConfiguration.AuthEndpoint},
            { RequestType.AccessTokenMetadata, _trueLayerConfiguration.AccessTokenMetadataEndpoint}
        }.ToImmutableDictionary();

        public string GetEndpoint(RequestType requestType)
        {
            return RequestEndpoints[requestType];
        }
    }
}