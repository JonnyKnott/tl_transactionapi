namespace TrueLayer.TransactionData.Models.Configurations
{
    public class TrueLayerConfiguration
    {
        public string DataApiUri { get; set; }
        public string AuthApiUri { get; set; }
        public string GetAccountsEndpoint { get; set; }
        public string GetTransactionsEndpoint { get; set; }
        public string AccessTokenMetadataEndpoint { get; set; }
        public string AuthEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccountIdPlaceholder { get; set; }
        
    }
}