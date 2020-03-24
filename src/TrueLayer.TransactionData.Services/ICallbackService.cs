using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services
{
    public interface ICallbackService
    {
        Task<ServiceResult> Process(CallbackRequest callbackData);
    }
}