﻿using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class AccountProvider
    {
        [JsonProperty("display_name")] 
        public string DisplayName { get; set; }
        
        [JsonProperty("provider_id")] 
        public string ProviderId { get; set; }
        
        [JsonProperty("logo_uri")] 
        public string LogoUri { get; set; }
    }
}