namespace ProjectManagement.Api.Middleware;


public static class MiddlewareExtensions
{
  
    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }


    public static IApplicationBuilder UseActiveUserValidation(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ActiveUserMiddleware>();
    }
}