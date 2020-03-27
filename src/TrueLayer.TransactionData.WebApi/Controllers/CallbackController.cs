using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Services;

namespace TrueLayer.TransactionData.WebApi.Controllers
{
    [ApiController]
    [Route( "api/v{version:apiVersion}/{controller}" )]
    public class CallbackController : ControllerBase
    {
        private readonly ICallbackService _service;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(ICallbackService service, ILogger<CallbackController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId, [FromQuery]CallbackRequest request)
        {
            _logger.LogInformation($"Processing new account access for user {userId}");
            
            var result = await _service.Process(userId, request);

            if (!result.Success)
            {
                return BadRequest();
            }
            
            return Ok();
        }
        
    }
}