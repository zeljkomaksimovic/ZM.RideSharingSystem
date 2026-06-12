using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using ZM.DriverService.Api.Application.UnitOfWork;

namespace ZM.DriverService.Api.Persistence.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DriverDbContext _dbContext;

        public UnitOfWork(DriverDbContext dbContext)
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
