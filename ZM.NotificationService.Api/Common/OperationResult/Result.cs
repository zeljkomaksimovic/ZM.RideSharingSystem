#nullable disable

namespace ZM.NotificationService.Api.Common.OperationResult
{
    public class Result<T> : Result
    {
        public T Data { get; set; }
        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                Data = data,
                IsSuccessful = true
            };
        }
        public static new Result<T> Failure(string message)
        {
            return new Result<T>
            {
                Message = message,
                IsSuccessful = false
            };
        }
    }

    public class Result
    {
        public bool IsSuccessful { get; init; }
        public string Message { get; init; }

        public static Result Failure(string message)
        {
            return new Result
            {
                Message = message,
                IsSuccessful = false
            };
        }

        public static Result Success()
        {
            return new Result
            {
                IsSuccessful = true
            };
        }
    }
}
