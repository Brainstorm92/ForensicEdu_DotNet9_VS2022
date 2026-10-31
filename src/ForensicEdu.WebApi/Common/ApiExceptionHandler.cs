using ForensicEdu.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.WebApi.Common;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested) return false;
        var status = exception switch
        {
            DatabaseCircuitOpenException => 503,
            BadHttpRequestException { StatusCode: 413 } => 413,
            ValidationException => 400,
            UnauthorizedAccessException => 401,
            DbUpdateConcurrencyException => 409,
            _ when IsDatabaseUnavailable(exception) => 503,
            DbUpdateException e when e.InnerException is Npgsql.PostgresException { SqlState: "23505" } => 409,
            _ => 500
        };
        var message = status switch {
            413 => "El archivo supera el tamaño permitido.",
            400 => "Revise los datos enviados.", 401 => "Se requiere una sesión autenticada.", 409 => "La operación entra en conflicto con un registro existente o fue modificada por otra solicitud.",
            503 => "El servicio no está disponible. Inténtalo de nuevo en unos segundos.", _ => "No se pudo completar la operación." };
        var problem = new ProblemDetails { Status = status, Title = message, Instance = context.Request.Path };
        problem.Extensions["message"] = message;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (exception is ValidationException validation)
            problem.Extensions["errors"] = validation.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
        if (status == 500) logger.LogError(exception, "Error no controlado. TraceId: {TraceId}", context.TraceIdentifier);
        else logger.LogWarning("Solicitud rechazada: {Status}, tipo {ErrorType}, TraceId {TraceId}", status, exception.GetType().Name, context.TraceIdentifier);
        context.Response.StatusCode = status;
        if (status == 503) context.Response.Headers.RetryAfter = exception is DatabaseCircuitOpenException circuit
            ? Math.Max(1,(int)Math.Ceiling(circuit.RetryAfter.TotalSeconds)).ToString(System.Globalization.CultureInfo.InvariantCulture) : "5";
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
    private static bool IsDatabaseUnavailable(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
            if (current is Npgsql.NpgsqlException pg && (pg.IsTransient || pg is not Npgsql.PostgresException)) return true;
        return false;
    }
}
