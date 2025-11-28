using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateSkuCommandHandler : IRequestHandler<CreateSkuCommand, SkuDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreateSkuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<SkuDto> Handle(CreateSkuCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe SKU para esta combinação
        Sku? existingSku = await _unitOfWork.Skus.GetByProductColorSizeAsync(request.Sku.ProductId, request.Sku.ColorId, request.Sku.SizeId, cancellationToken);
        if (existingSku != null)
            throw new InvalidOperationException("SKU já existe para esta combinação de Produto, Cor e Tamanho");

        // Verificar se as entidades relacionadas existem
        bool productExists = await _unitOfWork.Products.ExistsAsync(request.Sku.ProductId, cancellationToken);
        if (!productExists)
            throw new KeyNotFoundException($"Produto com ID {request.Sku.ProductId} não encontrado");

        bool colorExists = await _unitOfWork.Colors.ExistsAsync(request.Sku.ColorId, cancellationToken);
        if (!colorExists)
            throw new KeyNotFoundException($"Cor com ID {request.Sku.ColorId} não encontrada");

        bool sizeExists = await _unitOfWork.Sizes.ExistsAsync(request.Sku.SizeId, cancellationToken);
        if (!sizeExists)
            throw new KeyNotFoundException($"Tamanho com ID {request.Sku.SizeId} não encontrado");

        // Buscar entidades relacionadas - elas serão usadas apenas para validação e criação do SKU
        // O EF Core vai anexá-las automaticamente quando adicionarmos o Sku
        Product? product = await _unitOfWork.Products.GetByIdAsync(request.Sku.ProductId, cancellationToken);
        Color? color = await _unitOfWork.Colors.GetByIdAsync(request.Sku.ColorId, cancellationToken);
        Size? size = await _unitOfWork.Sizes.GetByIdAsync(request.Sku.SizeId, cancellationToken);

        if (product == null || color == null || size == null)
            throw new InvalidOperationException("Erro ao carregar entidades relacionadas");

        string tenantId = _tenantProvider.GetCurrentTenant();
        
        // Criar SKU com as entidades relacionadas
        // O problema é que o EF pode tentar inserir as entidades relacionadas
        // Vamos usar uma abordagem diferente: criar o Sku e depois anexar as entidades relacionadas
        Sku sku = new Sku(product, color, size, tenantId);

        // Adicionar o SKU - o EF deve reconhecer que as entidades relacionadas já existem
        // porque foram carregadas do mesmo contexto
        await _unitOfWork.Skus.AddAsync(sku, cancellationToken);
        
        // Antes de salvar, precisamos garantir que as entidades relacionadas não sejam inseridas
        // Isso deve ser feito no repositório ou no UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar o SKU com as relações para retornar o DTO completo
        Sku? createdSku = await _unitOfWork.Skus.GetByIdAsync(sku.Id, cancellationToken);
        if (createdSku == null)
            throw new InvalidOperationException("Erro ao criar SKU");

        return _mapper.Map<SkuDto>(createdSku);
    }
}

