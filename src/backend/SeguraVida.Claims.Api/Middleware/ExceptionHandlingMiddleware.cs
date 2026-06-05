using System.Net;
using FluentValidation;
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
            message = statusCode == HttpStatusCode.InternalServerError ? "Unexpected error." : exception.Message,
            errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}
