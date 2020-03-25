using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class CallbackRequest
    {
        public string Code { get; set; }
        
        public string Scope { get; set; }
        
        public string Error { get; set; }
        
        public string State { get; set; }

        public ICollection<Scope> GetScopes()
        {
            return Scope.Split(" ").Select(Enum.Parse<Scope>).ToList();
        }
    }
}