using System;
using System.Collections.Generic;
using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Models.ServiceModels
{
    public class AccountAccessContext
    {
        public string ProviderName { get; set; }
        public string CredentialsId { get; set; }
        public string UserId { get; set; }
        public string Code { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public ICollection<Scope> Scopes { get; set; }
        public DateTime AccessTokenExpiry { get; set; }

        public bool IsExpiredOrClose => AccessTokenExpiry <= DateTime.UtcNow.AddMinutes(1);
    }
}