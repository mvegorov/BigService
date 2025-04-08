#pragma warning disable IDE0066
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Gateway.Grpc;

public class GrpcExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GrpcExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode = GrpcStatusToHttpStatus(exception.StatusCode);

            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new
            {
                Code = exception.Status.StatusCode.ToString(),
                Message = exception.Status.Detail,
                Metadata = exception.Trailers.ToDictionary(k => k.Key, v => v.Value),
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
        catch (Exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync("An unexpected error occurred.");
        }
    }

    private static HttpStatusCode GrpcStatusToHttpStatus(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.NotFound:
                return HttpStatusCode.NotFound;

            case StatusCode.InvalidArgument:
                return HttpStatusCode.BadRequest;

            case StatusCode.PermissionDenied:
                return HttpStatusCode.Forbidden;

            case StatusCode.Unauthenticated:
                return HttpStatusCode.Unauthorized;

            case StatusCode.ResourceExhausted:
                return HttpStatusCode.TooManyRequests;

            case StatusCode.Aborted:
                return HttpStatusCode.Conflict;

            case StatusCode.Unavailable:
                return HttpStatusCode.ServiceUnavailable;

            case StatusCode.Internal:
                return HttpStatusCode.InternalServerError;

            default:
                return HttpStatusCode.InternalServerError;
        }
    }
}