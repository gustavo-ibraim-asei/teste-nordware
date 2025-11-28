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
    }
}


