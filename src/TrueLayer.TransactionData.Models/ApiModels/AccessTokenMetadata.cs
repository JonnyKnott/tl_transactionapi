using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Models.ApiModels
{

    public class AccessTokenMetadata
    {
        [JsonProperty("client_id")] 
        public string ClientId { get; set; }
        
        [JsonProperty("credentials_id")] 
        public string CredentialsId { get; set; }
        
        [JsonProperty("consent_status")] 
        public string ConsentStatus { get; set; }
        
        [JsonProperty("consent_status_updated_at")] 
        public DateTime ConsentUpdatedAt { get; set; }
        
        [JsonProperty("consent_created_at")] 
        public DateTime ConsentCreatedAt { get; set; }
        
        [JsonProperty("consent_expires_at")] 
        public DateTime ConsentExpiresAt { get; set; }
        public Provider Provider { get; set; }
        
        public List<Scope> Scopes { get; set; }
        
        [JsonProperty("privacy_policy")] 
        public string PrivacyPolicy { get; set; }
    }
}