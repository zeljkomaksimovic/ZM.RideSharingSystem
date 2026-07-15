namespace ZM.NotificationService.Api.Application.Templates
{
    public static class EmailTemplates
    {
        public static class Driver
        {
            public const string AssignedSubject = "Driver Assigned";

            public static string Assigned(Guid rideId, Guid driverId)
            {
                return
                    $"""
                    Dear Customer,

                    A driver has been assigned to your ride.

                    Ride Id: {rideId}
                    Driver Id: {driverId}

                    Your driver is on the way.

                    """;
            }
        }

        public static class Payment
        {
            public const string ReceiptSubject = "Payment Receipt";

            public static string Receipt(Guid paymentId, Guid rideId, decimal amount)
            {
                return
                    $"""
                    Dear Customer,

                    Your payment has been successfully processed.

                    Payment Id: {paymentId}
                    Ride Id: {rideId}
                    Amount: {amount:C}

                    """;
            }
        }

        public static class Ride
        {
            public const string CompletedSubject = "Ride Completed";
            public const string CancelledSubject = "Ride Cancelled";

            public static string Completed(Guid rideId)
            {
                return
                    $"""
                    Dear Customer,

                    Your ride has been completed successfully.

                    Ride Id: {rideId}

                    """;
            }

            public static string Cancelled(Guid rideId)
            {
                return
                    $"""
                    Dear Customer,

                    Your ride has been cancelled.

                    Ride Id: {rideId}

                    """;
            }
        }
    }
}