using System.Net;
using System.Text.Json;
using DigitalBanking.Domain.Exceptions;

namespace DigitalBanking.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var statusCode = exception switch
        {
            ExcepcionNegocio => (int)HttpStatusCode.BadRequest, // 400
            KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
            _ => (int)HttpStatusCode.InternalServerError // 500
        };

        response.StatusCode = statusCode;
        
        var errorResponse = new
        {
            success = false,
            status = statusCode,
            message = exception.Message
        };

        return response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}   