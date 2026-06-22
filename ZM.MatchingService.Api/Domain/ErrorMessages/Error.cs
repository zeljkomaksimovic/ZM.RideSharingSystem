namespace ZM.MatchingService.Api.Domain.ErrorMessages
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
