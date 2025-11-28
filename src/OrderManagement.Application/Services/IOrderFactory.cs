using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public interface IOrderFactory
{
    Order CreateOrder(CreateOrderDto dto, string tenantId);
}


