using CSharpAPI.Services;
using CSharpAPI.Services.Auth;
namespace CSharpAPI.Middleware
{


public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService) // Inject here instead
    {
        if (!context.Request.Headers.TryGetValue("API_KEY", out var apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "API key is required" });
            return;
        }

        var user = await authService.GetUserByApiKey(apiKey);
        if (user == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid API key" });
            return;
        }

        var path = context.Request.Path.Value?.Split('/')
            .Where(s => !string.IsNullOrEmpty(s))
            .Skip(2)  // Skip "api/v1"
            .FirstOrDefault();

        if (path == null || !await authService.HasAccess(user, path, context.Request.Method))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "Access denied" });
            return;
        }

        context.Items["User"] = user;
        await _next(context);
    }
}
}