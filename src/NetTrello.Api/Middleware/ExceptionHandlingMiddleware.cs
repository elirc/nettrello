using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NetTrello.Api.Domain;

namespace NetTrello.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(context, exception.StatusCode, exception.Code, exception.Message);
        }
        catch (BadHttpRequestException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "request.invalid", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            var detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.";
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "server.unhandled", detail);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string code, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
