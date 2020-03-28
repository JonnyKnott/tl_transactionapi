using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services;

namespace TrueLayer.TransactionData.WebApi.Controllers
{
    [ApiController]
    [Route( "api/v{version:apiVersion}/{controller}" )]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId)
        {
            try
            {
                var transactionsResult = await _transactionService.GetAllTransactions(userId);

                return GenerateResultFromServiceResult(transactionsResult);
            }
            catch (Exception ex)
            {
                //Log exception
                _logger.LogError(ex, "Error occurred processing a transactions request");
                return StatusCode(500);
            }

        }

        [HttpGet("{userId}/Summary")]
        public async Task<IActionResult> GetSummary(string userId, [FromQuery] bool detailed = false)
        {
            var transactionResult = await _transactionService.GetTransactionCategoryResponses(userId, detailed);

            return GenerateResultFromServiceResult(transactionResult);
        }

        private IActionResult GenerateResultFromServiceResult<TResultType>(
            ServiceObjectResult<TResultType> serviceObjectResult)
        {

            if (serviceObjectResult.Success)
                return Ok(serviceObjectResult.Result);

            if (serviceObjectResult.Errors.Any())
                return BadRequest(serviceObjectResult.Errors);

            return StatusCode(500);

        }

    }
}