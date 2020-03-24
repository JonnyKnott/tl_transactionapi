using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services
{
    public class CallbackService : ICallbackService
    {
        public CallbackService()
        {
            
        }
        public Task<ServiceResult> Process(CallbackRequest callbackData)
        {
            
            
            
            return Task.FromResult(ServiceResult.Succeeded());
        }
    }
}