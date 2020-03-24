using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class CallbackRequest
    {
        public string Code { get; set; }
        
        public string Scope { get; set; }

        public ICollection<string> GetScopes()
        {
            return Scope.Split(" ");
        }
    }
}