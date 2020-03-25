using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using TrueLayer.TransactionData.Models.ServiceModels;

namespace TrueLayer.TransactionData.Services
{
    public interface ITransactionService
    {
        Task<ServiceObjectResult<ICollection<Transaction>>> GetAllTransactions();
    }
}