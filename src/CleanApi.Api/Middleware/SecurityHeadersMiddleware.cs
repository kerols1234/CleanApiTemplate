namespace CleanApi.Api.Middleware;

/// <summary>Adds a conservative set of security response headers suitable for a JSON API.</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    // The docs and jobs dashboards serve HTML/assets that a locked-down CSP would break.
    private static readonly string[] UiPathPrefixes = ["/scalar", "/hangfire"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (UiPathPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix)))
        {
            await next(context);
            return;
        }

        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
        // An API returns no HTML, so lock the CSP right down.
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        headers.Remove("X-Powered-By");

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
