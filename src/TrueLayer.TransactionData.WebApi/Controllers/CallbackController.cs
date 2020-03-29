using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
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

            return GenerateResultFromServiceResult(result);
        }
        
        private IActionResult GenerateResultFromServiceResult(
            ServiceResult serviceResult)
        {

            if (serviceResult.Success)
                return Ok();

            if (serviceResult.Errors.Contains(ErrorCodeStrings.InternalError))
                return StatusCode(500, serviceResult.Errors);
            
            if (serviceResult.Errors.Contains(ErrorCodeStrings.BadRequestError))
                return BadRequest(serviceResult.Errors);
            
            if (serviceResult.Errors.Contains(ErrorCodeStrings.NotFoundError))
                return NotFound(serviceResult.Errors);

            if (serviceResult.Errors.Any())
                return StatusCode(500, serviceResult.Errors);

            return StatusCode(500, "The request could not be processed due to an unknown reason.");

        }
        
    }
}