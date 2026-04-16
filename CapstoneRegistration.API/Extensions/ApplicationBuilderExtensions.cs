using CapstoneRegistration.API.Middleware;

namespace CapstoneRegistration.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
