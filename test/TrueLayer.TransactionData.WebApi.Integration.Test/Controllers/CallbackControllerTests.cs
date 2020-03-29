using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using TrueLayer.TransactionData.WebApi.Test.Infrastructure;
using TrueLayer.TransactionData.WebApi.Test.Services;
using Xunit;

namespace TrueLayer.TransactionData.WebApi.Test.Controllers
{
    public class CallbackControllerTests : IClassFixture<ServerFactory>
    {
        private readonly ServerFactory _serverFactory;

        private const string CallbackEndpointUri = "api/v1/Callback/";

        public CallbackControllerTests(ServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        [Fact]
        public async void ShouldReturnOkForSuccessfulRequest()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{CallbackEndpointUri}{TestConstants.ValidUser}?code=1234");
            
            Assert.True(result.IsSuccessStatusCode);
        }
        
        [Fact]
        public async void ShouldReturn500ForUnforseenErrorRequest()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{CallbackEndpointUri}{TestConstants.ErrorUser}?code=1234");
            
            Assert.Equal(StatusCodes.Status500InternalServerError, (int)result.StatusCode);
        }
        
        [Fact]
        public async void ShouldReturn400ForBadRequest()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{CallbackEndpointUri}{TestConstants.BadRequestUser}?code=1234");
            
            Assert.Equal(StatusCodes.Status400BadRequest, (int)result.StatusCode);
        }
    }
}