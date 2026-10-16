using Microsoft.AspNetCore.Diagnostics;
using Otp.Domain.Exceptions;

namespace Otp.Api.Middleware;

// Turns a DomainException (e.g. "DateTime cannot be in the future") into a clean 400.
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException) return false; // others → default 500

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = { Status = 400, Title = "Invalid request", Detail = exception.Message }
        });
    }
}