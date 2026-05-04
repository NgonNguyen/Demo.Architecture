namespace Demo.Architecture.WebAPI.Middlewares;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserLoggingMiddleware> _logger;

    public UserLoggingMiddleware(RequestDelegate next, ILogger<UserLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst("sub")?.Value;
            var email = user.FindFirst("email")?.Value;

            _logger.LogInformation(
                "User {UserId} ({Email}) is calling {Method} {Path}",
                userId,
                email,
                context.Request.Method,
                context.Request.Path
            );
        }

        await _next(context);
    }
}
