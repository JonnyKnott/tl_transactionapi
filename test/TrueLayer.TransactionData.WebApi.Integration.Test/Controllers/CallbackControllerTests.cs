using System;
using System.Net;
using TrueLayer.TransactionData.WebApi.Test.Infrastructure;
using Xunit;

namespace TrueLayer.TransactionData.WebApi.Test.Controllers
{
    public class CallbackControllerTests : IClassFixture<ServerFactory<Program>>
    {
        private readonly ServerFactory<Program> _factory;

        private const string CallbackEndpointUri = "api/v1/Callback/12345";

        public CallbackControllerTests(ServerFactory<Program> factory)
        {
            _factory = factory;
        }

        // [Fact]
        // public async void Callback_Controller_Should_Accept_Correctly_Formed_Get()
        // {
        //     var code = Guid.NewGuid().ToString();
        //
        //     var requestUri = BuildRequestUri(code, new[] {"transactions"});
        //
        //     var client = _factory.CreateClient();
        //     var response = await client.GetAsync(requestUri);
        //     
        //     Assert.True(response.IsSuccessStatusCode);
        // }

        private string BuildRequestUri(string code, string[] scopes)
        {
            return $"{CallbackEndpointUri}?code={code}&scope={WebUtility.UrlEncode(string.Join(" ", scopes))}";
        }
    }
}