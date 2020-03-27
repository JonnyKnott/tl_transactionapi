using Newtonsoft.Json;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class TransactionMetadata
    {
        [JsonProperty("transaction_type")] 
        public string TransactionType { get; set; }
            
        [JsonProperty("provider_id")] 
        public string ProviderId { get; set; }
            
        [JsonProperty("counter_party_preferred_name")] 
        public string CounterPartyPreferredName { get; set; }
            
        [JsonProperty("provider_category")]
        public string ProviderCategory { get; set; }
    }
}