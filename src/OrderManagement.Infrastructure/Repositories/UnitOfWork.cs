using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderManagementDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(OrderManagementDbContext context, IOrderRepository orderRepository, IUserRepository userRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public IOrderRepository Orders => _orderRepository;
    public IUserRepository Users => _userRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle optimistic concurrency conflict
            throw new InvalidOperationException("O pedido foi modificado por outro processo. Por favor, atualize e tente novamente.");
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

