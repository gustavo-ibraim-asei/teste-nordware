using AutoMapper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.DTOs.ValueObjects;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>();

        CreateMap<CreateOrderItemDto, OrderItem>()
            .ConstructUsing(dto => new OrderItem(
                dto.ProductId,
                dto.ProductName,
                dto.Quantity,
                dto.UnitPrice));

        // Stock mappings
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<StockOffice, StockOfficeDto>();
        CreateMap<Color, ColorDto>();
        CreateMap<Size, SizeDto>();
        CreateMap<Sku, SkuDto>()
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
        CreateMap<Stock, StockDto>()
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity))
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku))
            .ForMember(dest => dest.StockOffice, opt => opt.MapFrom(src => src.StockOffice));

        // Price Table mappings
        CreateMap<PriceTable, PriceTableDto>();
        
        // Product Price mappings
        CreateMap<ProductPrice, ProductPriceDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.PriceTableName, opt => opt.MapFrom(src => src.PriceTable.Name));

        // Customer mappings
        CreateMap<Customer, CustomerDto>();
    }
}


