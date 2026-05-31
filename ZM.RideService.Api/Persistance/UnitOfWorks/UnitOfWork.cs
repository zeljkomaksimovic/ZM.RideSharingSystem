using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using ZM.RideService.Api.Application.UnitOfWork;

namespace ZM.RideService.Api.Persistance.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RideDbContext _dbContext;

        public UnitOfWork(RideDbContext dbContext)
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
