using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;
using TrueLayer.TransactionData.Services;

namespace TrueLayer.TransactionData.WebApi.Test.Services
{
    public class TestCallbackService : ICallbackService
    {
        public Task<ServiceResult> Process(string userId, CallbackRequest callbackData)
        {
            return Task.FromResult(GenerateResponse(userId));
        }

        private ServiceResult GenerateResponse(string userId)
        {
            switch (userId)
            {
                case TestConstants.ValidUser:
                    return ServiceResult.Succeeded();
                case TestConstants.ErrorUser:
                    return ServiceResult.Failed(new List<string> {ErrorCodeStrings.InternalError});
                case TestConstants.BadRequestUser:
                    return ServiceResult.Failed(new List<string> {ErrorCodeStrings.BadRequestError});
                case TestConstants.NotFoundUser:
                    return ServiceResult.Failed(new List<string> {ErrorCodeStrings.NotFoundError});
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
