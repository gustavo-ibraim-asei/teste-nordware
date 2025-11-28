using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using OrderManagement.API.Services;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Enums;
using BatchProcessResultDto = OrderManagement.Application.Commands.BatchProcessResultDto;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;
    private readonly INotificationService _notificationService;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger, INotificationService notificationService)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando pedido para cliente {CustomerId}", createOrderDto.CustomerId);

        CreateOrderCommand command = new CreateOrderCommand { Order = createOrderDto };
        OrderDto order = await _mediator.Send(command, cancellationToken);

        // Send notification
        string tenantId = HttpContext.Items["TenantId"]?.ToString() ?? "default";
        await _notificationService.NotifyOrderCreatedAsync(order, tenantId, cancellationToken);

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Obtém um pedido por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        GetOrderByIdQuery query = new GetOrderByIdQuery { OrderId = id };
        OrderDto? order = await _mediator.Send(query, cancellationToken);

        if (order == null)
            return NotFound($"Pedido com ID {id} não encontrado");

        return Ok(order);
    }

    /// <summary>
    /// Método para obter uma lista paginada de pedidos com filtros opcionais
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="status"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="sortBy"></param>
    /// <param name="sortDescending"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetOrders([FromQuery] int? customerId, [FromQuery] OrderStatus? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = "CreatedAt", [FromQuery] bool sortDescending = true, CancellationToken cancellationToken = default)
    {
        GetOrdersQuery query = new GetOrdersQuery
        {
            Query = new OrderQueryDto
            {
                CustomerId = customerId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            }
        };

        PagedResultDto<OrderDto> result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Método para atualizar o status de um pedido
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando status do pedido {OrderId} para {Status}", id, updateDto.Status);

        UpdateOrderStatusCommand command = new UpdateOrderStatusCommand
        {
            OrderId = id,
            Status = updateDto.Status
        };

        try
        {
            OrderDto order = await _mediator.Send(command, cancellationToken);

            // Send notification
            string tenantId = HttpContext.Items["TenantId"]?.ToString() ?? "default";
            await _notificationService.NotifyOrderStatusUpdatedAsync(order, tenantId, cancellationToken);

            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Método para cancelar um pedido
    /// </summary>
    /// <param name="id"></param>
    /// <param name="reason"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CancelOrder(int id, [FromQuery] string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelando pedido {OrderId}", id);

        CancelOrderCommand command = new CancelOrderCommand
        {
            OrderId = id,
            Reason = reason
        };

        try
        {
            OrderDto order = await _mediator.Send(command, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Método para processar um lote de pedidos
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchProcessResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchProcessResultDto>> ProcessOrdersBatch([FromBody] ProcessOrdersBatchCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando lote de {Count} pedidos", command.OrderIds.Count);

        BatchProcessResultDto result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Finaliza um pedido aplicando o frete (obrigatório)
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CompleteOrder(int id, [FromBody] CompleteOrderDto shippingInfo, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando pedido {OrderId} com frete", id);

        CompleteOrderCommand command = new CompleteOrderCommand
        {
            OrderId = id,
            ShippingInfo = shippingInfo
        };

        try
        {
            OrderDto order = await _mediator.Send(command, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}