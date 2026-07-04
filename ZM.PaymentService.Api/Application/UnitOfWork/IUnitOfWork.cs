using System.Data;

namespace ZM.PaymentService.Api.Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        IDbTransaction BeginTransaction();
    }
}
