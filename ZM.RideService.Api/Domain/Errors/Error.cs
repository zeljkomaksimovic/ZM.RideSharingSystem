namespace ZM.RideService.Api.Domain.Errors
{
    public class Error
    {
        public Error(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public string ErrorCode { get; }
        public string ErrorMessage { get; }

    }
}
