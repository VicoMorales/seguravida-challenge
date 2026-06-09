using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Common;
using SeguraVida.Claims.Domain.Common;

namespace SeguraVida.Claims.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            DomainException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            DbUpdateConcurrencyException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "UnexpectedError {EventType}", "UnexpectedError");
        }
        else
        {
            _logger.LogWarning("HandledError {EventType} {StatusCode} {Message}", "HandledError", (int)statusCode, exception.Message);
        }

        var errors = exception is ValidationException validationException
            ? validationException.Errors.Select(error => error.ErrorMessage).ToArray()
            : [];

        var response = new
        {
            traceId = context.TraceIdentifier,
            statusCode = (int)statusCode,
            message = ClientMessage(exception, statusCode),
            errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static string ClientMessage(Exception exception, HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            return "Unexpected error.";
        }

        if (exception is DbUpdateConcurrencyException)
        {
            return "La informacion del siniestro fue modificada. Actualiza la pantalla e intentalo nuevamente.";
        }

        if (exception is DomainException or NotFoundException)
        {
            return exception.Message switch
            {
                "Policy was not found." => "No se encontro la poliza.",
                "Claim was not found." => "No se encontro el siniestro.",
                "The policy must be active on the incident date." => "La poliza debe estar activa en la fecha del incidente.",
                "Incident date cannot be after report date." => "La fecha del incidente no puede ser posterior a la fecha de reporte.",
                "Claimed amount must be greater than zero." => "El monto reclamado debe ser mayor a cero.",
                "Claimed amount cannot exceed policy insured amount." => "El monto reclamado no puede exceder la suma asegurada de la poliza.",
                "Claim description is required." => "La descripcion del siniestro es obligatoria.",
                "Approved amount is required to approve a claim." => "El monto aprobado es obligatorio para aprobar el siniestro.",
                "Approved amount cannot exceed claimed amount." => "El monto aprobado no puede exceder el monto reclamado.",
                "Adjustment notes are required to approve a claim." => "Las notas de peritaje son obligatorias para aprobar el siniestro.",
                "Adjustment notes are required to reject a claim." => "Las notas de peritaje son obligatorias para rechazar el siniestro.",
                "Status change user is required." => "No se pudo identificar al usuario que realiza el cambio.",
                "A similar claim already exists for the same policy and incident date." => "Ya existe un siniestro similar para la misma poliza y fecha de incidente.",
                _ when exception.Message.StartsWith("Invalid claim status transition", StringComparison.Ordinal) =>
                    "La accion no es valida para el estado actual del siniestro.",
                _ => exception.Message
            };
        }

        return exception.Message;
    }
}
