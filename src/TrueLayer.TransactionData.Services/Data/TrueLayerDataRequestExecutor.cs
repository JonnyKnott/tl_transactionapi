﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.Configurations;
using TrueLayer.TransactionData.Models.Enums;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services.Data
{
    public class TrueLayerDataRequestExecutor : ITrueLayerDataRequestExecutor
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IRequestTypeEndpointService _requestTypeEndpointService;
        private readonly TrueLayerRequestConfiguration _requestConfiguration;

        public TrueLayerDataRequestExecutor(IHttpClientFactory clientFactory, IRequestTypeEndpointService requestTypeEndpointService, TrueLayerRequestConfiguration requestConfiguration)
        {
            _clientFactory = clientFactory;
            _requestTypeEndpointService = requestTypeEndpointService;
            _requestConfiguration = requestConfiguration;
        }

        private HttpClient CreateClient()
        {
            return _clientFactory.CreateClient(HttpClients.TrueLayerDataClientName);
        }

        private HttpRequestMessage BuildDataRequest(string uri, string accessToken, HttpMethod httpMethod)
        {
            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri(uri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            requestMessage.Method = httpMethod;

            return requestMessage;
        }

        public async Task<ServiceObjectResult<TrueLayerListResponse<Account>>> GetAccounts(string accessToken)
        {
            var client = CreateClient();

            var request =
                BuildDataRequest(
                    $"{client.BaseAddress}{_requestTypeEndpointService.GetEndpoint(RequestType.GetAccounts)}",
                    accessToken, HttpMethod.Get);

            using (var response = await client.SendAsync(request))
            {
                return await ProcessResponse<TrueLayerListResponse<Account>>(response, RequestType.GetAccounts, request.RequestUri.ToString());
            }
        }

        public async Task<ServiceObjectResult<TrueLayerListResponse<Transaction>>> GetTransactions(string accessToken, string accountId)
        {
            var client = _clientFactory.CreateClient(HttpClients.TrueLayerDataClientName);

            var endpoint = _requestTypeEndpointService.GetEndpoint(RequestType.GetTransactions)
                .Replace($"{_requestConfiguration.AccountIdPlaceholder}", accountId);

            var request = BuildDataRequest(
                $"{client.BaseAddress}{endpoint}",
                accessToken, HttpMethod.Get
            );
            
            using (var response = await client.SendAsync(request))
            {
                return await ProcessResponse<TrueLayerListResponse<Transaction>>(response, RequestType.GetTransactions, request.RequestUri.ToString());
            }
        }

        public async Task<ServiceObjectResult<AuthResponse>> RefreshAccountAccess(AccountAccessContext accountContext)
        {
            var client = _clientFactory.CreateClient(HttpClients.TrueLayerAuthClientName);

            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _requestConfiguration.ClientId),
                new KeyValuePair<string, string>("client_secret", _requestConfiguration.ClientSecret),
                new KeyValuePair<string, string>("refresh_token", accountContext.RefreshToken)
            });

            var requestEndpoint = _requestTypeEndpointService.GetEndpoint(RequestType.RefreshAccess);

            using (var response =
                await client.PostAsync(requestEndpoint, formContent))
            {
                return await ProcessResponse<AuthResponse>(response, RequestType.RefreshAccess,
                    $"{client.BaseAddress}{requestEndpoint}");
            }
        }

        public async Task<ServiceObjectResult<AuthResponse>> ExchangeCode(AccountAccessContext accountContext)
        {
            var client = _clientFactory.CreateClient(HttpClients.TrueLayerAuthClientName);
            
            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", _requestConfiguration.ClientId),
                new KeyValuePair<string, string>("client_secret", _requestConfiguration.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", $"{_requestConfiguration.CallbackUrlBase}{accountContext.UserId}"),
                new KeyValuePair<string, string>("code", accountContext.Code)
            });
            
            var requestEndpoint = _requestTypeEndpointService.GetEndpoint(RequestType.ExchangeCode);

            using (var response =
                await client.PostAsync(requestEndpoint, formContent))
            {
                return await ProcessResponse<AuthResponse>(response, RequestType.ExchangeCode,
                    $"{client.BaseAddress}{requestEndpoint}");
            }
        }

        public async Task<ServiceObjectResult<TrueLayerListResponse<AccessTokenMetadata>>> GetAccessTokenMetadata(string accessToken)
        {
            var client = _clientFactory.CreateClient(HttpClients.TrueLayerDataClientName);
            
            var request =
                BuildDataRequest(
                    $"{client.BaseAddress}{_requestTypeEndpointService.GetEndpoint(RequestType.AccessTokenMetadata)}",
                    accessToken, HttpMethod.Get);

            using (var response = await client.SendAsync(request))
            {
                return await ProcessResponse<TrueLayerListResponse<AccessTokenMetadata>>(response, RequestType.AccessTokenMetadata, request.RequestUri.ToString());
            }
        }

        private async Task<ServiceObjectResult<TResultType>> ProcessResponse<TResultType>(HttpResponseMessage response, RequestType requestType, string requestUri)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if(!response.IsSuccessStatusCode)
                return ServiceObjectResult<TResultType>.Failed(default, GenerateErrorsForUnsuccessfulRequest(response, requestType, requestUri));

            var result = JsonConvert.DeserializeObject<TResultType>(responseContent);
                
            return ServiceObjectResult<TResultType>.Succeeded(result);
        }
        
        private ICollection<string> GenerateErrorsForUnsuccessfulRequest(HttpResponseMessage response, RequestType requestType, string requestUri)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.InternalServerError:
                    return new[]
                    {
                        $"An error occurred whilst making the {requestType} request to {requestUri}"
                    };
                case HttpStatusCode.Unauthorized:
                    return new[]
                    {
                        ErrorMessages.AccountAccessExpired
                    };
                case HttpStatusCode.Forbidden:
                    return new[]
                    {
                        $"The bearer token is either invalid or expired"
                    };
                default:
                    return new[]
                    {
                        $"An unrecognised error occurred returning response code {response.StatusCode} - RequestUri: {requestUri}"
                    };
            }
        }
    }
}