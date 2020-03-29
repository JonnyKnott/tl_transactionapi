using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services;

namespace TrueLayer.TransactionData.WebApi.Test.Services
{
    public class TestTransactionService : ITransactionService
    {
        public Task<ServiceObjectResult<ICollection<Transaction>>> GetAllTransactions(string userId)
        {
            return Task.FromResult(GenerateResponseForType<ICollection<Transaction>>(userId));
        }

        public Task<ServiceObjectResult<TransactionCategoryResponse>> GetTransactionCategoryResponses(string userId, bool detailed = false)
        {
            return Task.FromResult(GenerateResponseForType<TransactionCategoryResponse>(userId));
        }

        private ServiceObjectResult<TReturnType> GenerateResponseForType<TReturnType>(string userId)
        where TReturnType: class
        {
            switch (userId)
            {
                case TestConstants.ValidUser:
                    return ServiceObjectResult<TReturnType>.Succeeded(null);
                case TestConstants.ErrorUser:
                    return ServiceObjectResult<TReturnType>.Failed(null, new List<string>{ ErrorCodeStrings.InternalError });
                case TestConstants.BadRequestUser:
                    return ServiceObjectResult<TReturnType>.Failed(null, new List<string>{ ErrorCodeStrings.BadRequestError });
                case TestConstants.NotFoundUser:
                    return ServiceObjectResult<TReturnType>.Failed(null, new List<string>{ ErrorCodeStrings.NotFoundError });
                default:
                    throw new NotImplementedException();
            }
        }
    }
}