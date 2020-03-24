using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Services;

namespace TrueLayer.TransactionData.WebApi.Controllers
{
    [ApiController]
    [Route( "api/v{version:apiVersion}/{controller}" )]
    public class CallbackController : ControllerBase
    {
        private readonly ICallbackService _service;

        public CallbackController(ICallbackService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Post([FromQuery]CallbackRequest request)
        {
            var result = await _service.Process(request);

            if (!result.Success)
            {
                return BadRequest();
            }
            
            return Ok();
        }
        
    }
}