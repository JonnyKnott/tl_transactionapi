using Microsoft.AspNetCore.Http;
using TrueLayer.TransactionData.WebApi.Test.Infrastructure;
using TrueLayer.TransactionData.WebApi.Test.Services;
using Xunit;

namespace TrueLayer.TransactionData.WebApi.Test.Controllers
{
    public class TransactionControllerTests : IClassFixture<ServerFactory>
    {
        private readonly ServerFactory _serverFactory;

        private const string TransactionEndpointUri = "api/v1/Transaction/";

        public TransactionControllerTests(ServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        [Fact]
        public async void ShouldReturnOkForSuccessfulRequest()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{TransactionEndpointUri}{TestConstants.ValidUser}");
            
            Assert.True(result.IsSuccessStatusCode);
        }
        
        [Fact]
        public async void ShouldReturn500ForUnforseenErrorRequest()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{TransactionEndpointUri}{TestConstants.ErrorUser}");
            
            Assert.Equal(StatusCodes.Status500InternalServerError, (int)result.StatusCode);
        }
        
        [Fact]
        public async void ShouldReturn400ForBadRequest()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{TransactionEndpointUri}{TestConstants.BadRequestUser}");
            
            Assert.Equal(StatusCodes.Status400BadRequest, (int)result.StatusCode);
        }
        
        [Fact]
        public async void ShouldReturn404ForNotFound()
        {
            var client = _serverFactory.CreateClient();

            var result = await client.GetAsync($"{TransactionEndpointUri}{TestConstants.NotFoundUser}");
            
            Assert.Equal(StatusCodes.Status404NotFound, (int)result.StatusCode);
        }
    }
    
}