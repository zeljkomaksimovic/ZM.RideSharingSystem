using System;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.UnitTests.Builders
{
    internal static class RiderInfoBuilder
    {
        public static RiderInfo Build(Guid? id = null, string firstName = "First", string lastName = "Last", string email = "email@example.com")
        {
            return new RiderInfo(id ?? Guid.NewGuid(), firstName, lastName, email);
        }
    }
}
