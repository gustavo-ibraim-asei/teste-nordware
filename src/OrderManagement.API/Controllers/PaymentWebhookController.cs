using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Enums;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(IMediator mediator, ILogger<PaymentWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Recebe webhook de atualização de pagamento
    /// </summary>
    [HttpPost("payment-update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceivePaymentUpdate([FromBody] PaymentWebhookDto webhookDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Webhook de pagamento recebido para pedido {OrderId}. Status: {Status}", webhookDto.OrderId, webhookDto.PaymentStatus);

        try
        {
            // Mapear status de pagamento para status do pedido
            OrderStatus orderStatus = webhookDto.PaymentStatus.ToLower() switch
            {
                "paid" or "approved" or "confirmed" => OrderStatus.Confirmed,
                "pending" or "processing" => OrderStatus.Pending,
                "cancelled" or "refunded" or "rejected" => OrderStatus.Cancelled,
                _ => OrderStatus.Pending
            };

            // Atualizar status do pedido
            UpdateOrderStatusCommand command = new UpdateOrderStatusCommand
            {
                OrderId = webhookDto.OrderId,
                Status = orderStatus
            };

            OrderDto order = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Status do pedido {OrderId} atualizado para {Status} baseado no webhook de pagamento", order.Id, order.Status);

            return Ok(new
            {
                message = "Atualização de pagamento processada com sucesso",
                orderId = order.Id,
                orderStatus = order.Status
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Pedido {OrderId} não encontrado para webhook de pagamento", webhookDto.OrderId);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao processar webhook de pagamento para pedido {OrderId}: {Message}", webhookDto.OrderId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar webhook de pagamento para pedido {OrderId}", webhookDto.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Falha ao processar atualização de pagamento" });
        }
    }
}

/// <summary>
/// DTO para receber atualizações de pagamento via webhook
/// </summary>
public class PaymentWebhookDto
{
    /// <summary>
    /// ID do pedido
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Status do pagamento (paid, approved, pending, cancelled, refunded, rejected)
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>
    /// ID da transação no gateway de pagamento
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Valor do pagamento
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Data/hora do processamento
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}

