using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace TransSolutions.Exceptions;

public static class CustomExceptionExtensions
{
    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(err =>
        {
            err.Run(async ctx =>
            {
                var feature = ctx.Features.Get<IExceptionHandlerFeature>();
                if (feature is null) return;

                var exception = feature.Error;

                var (statusCode, statusTitle, reason) = exception switch
                {
                    // 1. Database / Transient Failures
                    InvalidOperationException ex when ex.Message.Contains("transient failure") => 
                        (HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The database is temporarily busy or down."),
                    
                    _ when exception.InnerException is PostgresException { SqlState: "57P01" or "57P03" } => 
                        (HttpStatusCode.ServiceUnavailable, "Service Unavailable", "Database connection was terminated."),

                    //Other failures
                    KeyNotFoundException => 
                        (HttpStatusCode.NotFound, "Not Found", "The requested resource does not exist."),

                    UnauthorizedAccessException => 
                        (HttpStatusCode.Forbidden, "Forbidden", "You do not have permission to access this resource."),

                    ArgumentException ex => 
                        (HttpStatusCode.BadRequest, "Bad Request", ex.Message),

                    _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
                };

                ctx.Response.StatusCode = (int)statusCode;
                ctx.Response.ContentType = "application/json";

                await ctx.Response.WriteAsJsonAsync(new
                {
                    Status = statusTitle,
                    Code = (int)statusCode,
                    Reason = reason,
                    /*
                Details = app.Environment.IsDevelopment() ? exception.Message : null
            */
                });
            });
        });
    }
}