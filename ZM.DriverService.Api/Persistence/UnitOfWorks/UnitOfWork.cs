using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using ZM.DriverService.Api.Application.DomainEventCollector;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DriverDbContext _dbContext;
    private readonly IDomainEventCollector _domainEventCollector;
    private readonly IPublisher _publisher;

    public UnitOfWork(
        DriverDbContext dbContext,
        IDomainEventCollector domainEventCollector,
        IPublisher publisher)
    {
        _dbContext = dbContext;
        _domainEventCollector = domainEventCollector;
        _publisher = publisher;
    }

    public IDbTransaction BeginTransaction()
    {
        var transaction = _dbContext.Database.BeginTransaction();
        return transaction.GetDbTransaction();
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in _domainEventCollector.GetDomainEvents())
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        _domainEventCollector.ClearDomainEvents();
    }
}