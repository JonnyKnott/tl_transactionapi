using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class AuthResponse
    {
        [JsonProperty("access_token")] 
        public string AccessToken { get; set; }
        
        [JsonProperty("expires_in")] 
        public int ExpiresIn { get; set; }
        
        [JsonProperty("token_type")] 
        public string TokenType { get; set; }
        
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        public string Scopes { get; set; }
    }
}