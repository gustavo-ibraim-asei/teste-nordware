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
    private readonly ISkuRepository _skuRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IRepository<Domain.Entities.Product> _productRepository;
    private readonly IRepository<Domain.Entities.StockOffice> _stockOfficeRepository;
    private readonly IRepository<Domain.Entities.Color> _colorRepository;
    private readonly IRepository<Domain.Entities.Size> _sizeRepository;
    private readonly IPriceTableRepository _priceTableRepository;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly ICustomerRepository _customerRepository;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        OrderManagementDbContext context,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ISkuRepository skuRepository,
        IStockRepository stockRepository,
        IRepository<Domain.Entities.Product> productRepository,
        IRepository<Domain.Entities.StockOffice> stockOfficeRepository,
        IRepository<Domain.Entities.Color> colorRepository,
        IRepository<Domain.Entities.Size> sizeRepository,
        IPriceTableRepository priceTableRepository,
        IProductPriceRepository productPriceRepository,
        ICustomerRepository customerRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _skuRepository = skuRepository;
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _stockOfficeRepository = stockOfficeRepository;
        _colorRepository = colorRepository;
        _sizeRepository = sizeRepository;
        _priceTableRepository = priceTableRepository;
        _productPriceRepository = productPriceRepository;
        _customerRepository = customerRepository;
    }

    public IOrderRepository Orders => _orderRepository;
    public IUserRepository Users => _userRepository;
    public ISkuRepository Skus => _skuRepository;
    public IStockRepository Stocks => _stockRepository;
    public IRepository<Domain.Entities.Product> Products => _productRepository;
    public IRepository<Domain.Entities.StockOffice> StockOffices => _stockOfficeRepository;
    public IRepository<Domain.Entities.Color> Colors => _colorRepository;
    public IRepository<Domain.Entities.Size> Sizes => _sizeRepository;
    public IPriceTableRepository PriceTables => _priceTableRepository;
    public IProductPriceRepository ProductPrices => _productPriceRepository;
    public ICustomerRepository Customers => _customerRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Tratar conflito de concorrência otimista
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

