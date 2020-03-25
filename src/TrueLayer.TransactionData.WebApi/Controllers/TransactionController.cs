using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services;

namespace TrueLayer.TransactionData.WebApi.Controllers
{
    [ApiController]
    [Route( "api/v{version:apiVersion}/{controller}" )]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var transactionsResult = await _transactionService.GetAllTransactions();

                return GenerateResultFromServiceResult(transactionsResult);
            }
            catch (Exception ex)
            {
                //Log exception
                return StatusCode(500);
            }

        }

        private IActionResult GenerateResultFromServiceResult<TResultType>(ServiceObjectResult<TResultType> serviceObjectResult)
        {
            
                if(serviceObjectResult.Success)
                    return Ok(serviceObjectResult.Result);

                if (serviceObjectResult.Errors.Any())
                    return StatusCode(500, serviceObjectResult.Errors);

                return BadRequest();

        }
        
    }
}