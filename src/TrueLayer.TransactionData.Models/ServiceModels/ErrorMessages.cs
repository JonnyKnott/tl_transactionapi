using System.Net;

namespace TrueLayer.TransactionData.Models.ServiceModels
{
    public static class ErrorCodeStrings
    {
        public static string InternalError = HttpStatusCode.InternalServerError.ToString();
        public static string BadRequestError = HttpStatusCode.BadRequest.ToString();
        public static string NotFoundError = HttpStatusCode.NotFound.ToString();
    }
    public static class ErrorMessages
    {
        public const string CallbackStatedAccessDenied = "The user did not grant the requested permissions to access data api";

        public const string AccountAccessExpired =
            "The access for your accounts has expired. Please re-register your accounts. In the future, allowing offline access will prevent this from happening.";

        public const string UserNotFound =
            "No connected accounts were found for user. Connect a bank account to use the API";

        public const string FailedToObtainAccessToken = "Failed to obtain an access token for your accounts. Please try again.";
        public const string FailedToObtainAccessTokenMetadata = "Failed to obtain an access token metadata for your accounts. Please try again.";
    }
}