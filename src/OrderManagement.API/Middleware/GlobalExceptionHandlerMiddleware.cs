using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace OrderManagement.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada ocorreu. RequestId: {RequestId}", context.TraceIdentifier);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode code = HttpStatusCode.InternalServerError;
        string? result;
        switch (exception)
        {
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new
                {
                    error = "Não Encontrado",
                    message = exception.Message,
                    requestId = context.TraceIdentifier
                });
                break;

            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new
                {
                    error = "Requisição Inválida",
                    message = exception.Message,
                    requestId = context.TraceIdentifier
                });
                break;

            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new
                {
                    error = "Operação Inválida",
                    message = exception.Message,
                    requestId = context.TraceIdentifier
                });
                break;

            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new
                {
                    error = "Não Autorizado",
                    message = "Você não está autorizado a realizar esta ação.",
                    requestId = context.TraceIdentifier
                });
                break;

            case DbUpdateConcurrencyException:
                code = HttpStatusCode.Conflict;
                result = JsonSerializer.Serialize(new
                {
                    error = "Conflito de Concorrência",
                    message = "O recurso foi modificado por outro processo. Por favor, atualize e tente novamente.",
                    requestId = context.TraceIdentifier
                });
                break;

            case ValidationException validationEx:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new
                {
                    error = "Erro de Validação",
                    message = "Os dados fornecidos são inválidos.",
                    errors = validationEx.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        message = e.ErrorMessage,
                        attemptedValue = e.AttemptedValue
                    }),
                    requestId = context.TraceIdentifier
                });
                break;

            default:
                result = JsonSerializer.Serialize(new
                {
                    error = "Erro Interno do Servidor",
                    message = "Ocorreu um erro ao processar sua requisição.",
                    requestId = context.TraceIdentifier
                });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

