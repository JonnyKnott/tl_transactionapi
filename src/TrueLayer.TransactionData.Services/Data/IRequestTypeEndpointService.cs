using TrueLayer.TransactionData.Models.Enums;

namespace TrueLayer.TransactionData.Services.Data
{
    public interface IRequestTypeEndpointService
    {
        string GetEndpoint(RequestType requestType);
    }
}