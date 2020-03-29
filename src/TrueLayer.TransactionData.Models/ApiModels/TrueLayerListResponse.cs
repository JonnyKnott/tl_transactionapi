using System.Collections.Generic;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class TrueLayerListResponse<TResponseValue>
    {
        public ICollection<TResponseValue> Results { get; set; }
        public string Status { get; set; }
    }
}