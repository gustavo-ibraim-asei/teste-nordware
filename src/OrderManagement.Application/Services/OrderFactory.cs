using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public class OrderFactory : IOrderFactory
{
    public Order CreateOrder(CreateOrderDto dto, string tenantId)
    {
        Address address = new Address(dto.ShippingAddress.Street, dto.ShippingAddress.Number, dto.ShippingAddress.Neighborhood, dto.ShippingAddress.City, dto.ShippingAddress.State, dto.ShippingAddress.ZipCode, dto.ShippingAddress.Complement);

        List<OrderItem> items = dto.Items.Select(item => new OrderItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice)).ToList();

        return new Order(dto.CustomerId, address, items, tenantId);
    }
}

