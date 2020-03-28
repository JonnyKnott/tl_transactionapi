using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class Transaction
    {
        public DateTime Timestamp { get; set; }

        public string Description { get; set; }

        [JsonProperty("transaction_type")] 
        public TransactionType TransactionType { get; set; }

        [JsonProperty("transaction_category")]
        public TransactionCategory TransactionCategory { get; set; }

        [JsonProperty("transaction_classification")]
        public List<string> TransactionClassifications { get; set; }

        [JsonProperty("merchant_name")] 
        public string MerchantName { get; set; }
        public double Amount { get; set; }

        public string Currency { get; set; }

        [JsonProperty("transaction_id")] 
        public string TransactionId { get; set; }

        [JsonProperty("meta")] 
        public TransactionMetadata TransactionMetadata { get; set; }
    }
}
