using FluentValidation;
using IdentityService.Common.Responses;
using Microsoft.AspNetCore.Diagnostics;

namespace IdentityService.Common.Handlers
{
    // Catches anything that escapes an endpoint and writes it back as the unified
    // ApiResponse envelope, so clients never see a raw stack trace or ProblemDetails.
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var response = exception switch
            {
                ValidationException validation => ApiResponse.Fail(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    validation.Errors
                        .Select(e => new ApiError(e.ErrorMessage, e.PropertyName))
                        .ToList()),

                _ => ApiResponse.Fail(
                    "An unexpected error occurred",
                    StatusCodes.Status500InternalServerError)
            };

            // Only 5xx is worth an error-level log; validation is expected client input.
            if (response.Code >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = response.Code;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
