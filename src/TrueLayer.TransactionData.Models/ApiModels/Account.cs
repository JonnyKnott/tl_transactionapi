using System;
using Newtonsoft.Json;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class Account
    {
        [JsonProperty("update_timestamp")] 
        public DateTime UpdateTimestamp { get; set; }
        
        [JsonProperty("account_id")] 
        public string AccountId { get; set; }
        
        [JsonProperty("account_type")] 
        public string AccountType { get; set; }
        
        [JsonProperty("display_name")] 
        public string DisplayName { get; set; }
        
        public string Currency { get; set; }
        
        [JsonProperty("account_number")] 
        public AccountNumber AccountNumber { get; set; }
        public Provider Provider { get; set; }
    }
}