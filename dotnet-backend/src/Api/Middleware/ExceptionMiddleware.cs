using System.Net;
using System.Text.Json;

namespace Api.Middleware;

/// <summary>
/// Middleware for catching unhandled exceptions, logging them, and returning a consistent error response as JSON.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger for logging exception details.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Executes the middleware logic, catching exceptions, and returning a JSON error response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next middleware/component in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception with stack trace
            _logger.LogError(ex, "An unhandled exception occurred.");
            // Return standardized error response to client
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles caught exceptions and formats the HTTP response as JSON with the appropriate status code and details.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The exception that was caught.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous write to the response.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Set the response content type to JSON
        context.Response.ContentType = "application/json";

        // Map known exception types to appropriate HTTP status codes, fallback to 500
        var statusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        // Construct an error response object
        var response = new
        {
            statusCode = statusCode,
            message = exception.Message,
            // Provide a generic error message for 500 Internal Server Error to avoid leaking sensitive details
            details = statusCode == (int)HttpStatusCode.InternalServerError ? "Internal Server Error" : exception.Message
        };

        // Serialize the error response as JSON and write it to the response stream
        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
