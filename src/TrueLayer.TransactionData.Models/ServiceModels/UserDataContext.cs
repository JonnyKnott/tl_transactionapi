using System.Collections.Generic;

namespace TrueLayer.TransactionData.Models.ServiceModels
{
    public class UserDataContext
    {
        public string UserId { get; set; }
        public ICollection<AccountAccessContext> AccessContexts { get; set; }
    }
}