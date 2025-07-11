using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CodexTest.Exceptions;
using FluentValidation;

namespace CodexTest.Extensions;

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var logger = context.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                var exception = exceptionHandlerPathFeature?.Error;

                logger?.CreateLogger("GlobalExceptionHandler")
                    .LogError(exception, "Unhandled exception");

                int statusCode;
                string title;
                string detail = exception?.Message ?? "An error occurred.";

                // Map known exceptions to status codes and titles
                switch (exception)
                {
                    case NotFoundException:
                        statusCode = StatusCodes.Status404NotFound;
                        title = "Resource Not Found";
                        break;
                    case ValidationException:
                        statusCode = StatusCodes.Status400BadRequest;
                        title = "Validation Error";
                        break;
                    case UnauthorizedException:
                        statusCode = StatusCodes.Status401Unauthorized;
                        title = "Unauthorized";
                        break;
                    case ForbiddenException:
                        statusCode = StatusCodes.Status403Forbidden;
                        title = "Forbidden";
                        break;
                    // Add more custom exception mappings as needed
                    default:
                        statusCode = StatusCodes.Status500InternalServerError;
                        title = "An unexpected error occurred!";
                        break;
                }

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = context.Request.Path
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
        return app;
    }
}
