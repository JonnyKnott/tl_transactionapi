using System.Collections.Generic;
using System.Threading.Tasks;
using TrueLayer.TransactionData.Models.ApiModels;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services
{
    public interface IAccountService
    {
        Task<ServiceObjectResult<ICollection<Account>>> GetAccountsInCurrentContext();
    }
}