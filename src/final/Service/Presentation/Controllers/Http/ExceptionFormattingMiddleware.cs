using Microsoft.AspNetCore.Http;

namespace Presentation.Controllers.Http;

public class ExceptionFormattingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            int statusCode = e switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError,
            };

            var errorResponse = new ErrorResponse
            {
                Message = e.Message,
                Details = e.InnerException?.Message,
                StatusCode = statusCode,
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}