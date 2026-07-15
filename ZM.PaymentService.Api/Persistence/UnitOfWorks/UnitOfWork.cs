using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using ZM.PaymentService.Api.Application.UnitOfWork;

namespace ZM.PaymentService.Api.Persistence.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _dbContext;

        public UnitOfWork(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IDbTransaction BeginTransaction()
        {
            var transaction = _dbContext.Database.BeginTransaction();
            return transaction.GetDbTransaction();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
