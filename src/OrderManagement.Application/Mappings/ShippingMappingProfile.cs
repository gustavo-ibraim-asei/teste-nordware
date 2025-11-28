using AutoMapper;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Mappings;

public class ShippingMappingProfile : Profile
{
    public ShippingMappingProfile()
    {
        CreateMap<ShippingOption, ShippingOptionDto>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => 
                src.IsFree 
                    ? $"Frete Grátis - {src.CarrierName} {src.ShippingType} ({src.EstimatedDays} dias úteis)"
                    : src.IsSameDay
                    ? $"{src.CarrierName} {src.ShippingType} - Entrega no mesmo dia"
                    : $"{src.CarrierName} {src.ShippingType} - {src.EstimatedDays} dias úteis"));
    }
}


