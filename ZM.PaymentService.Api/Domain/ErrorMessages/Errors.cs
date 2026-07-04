namespace ZM.PaymentService.Api.Domain.ErrorMessages
{
    public static class Errors
    {
        public static class Payment
        {
            public static Error PaymentNotFound() => new("PAY0001", "Payment not found.");
            public static Error PaymentAlreadyCompleted() => new("PAY0002", "Payment is already completed.");
            public static Error PaymentFailed() => new("PAY0002", "Payment is failed.");
            public static Error PaymentCannotBeProcessed() => new("PAY0003", "Payment could not be processed.");
        }
    }
}
