using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class AccountNumber
    {
        public string Iban { get; set; }
        
        [JsonProperty("swift_bic")] 
        public string SwiftBic { get; set; }
        public string Number { get; set; }
        
        [JsonProperty("sort_code")] 
        public string SortCode { get; set; }
    }
}