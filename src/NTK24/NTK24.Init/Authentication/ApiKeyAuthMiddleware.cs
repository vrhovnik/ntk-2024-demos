using System.Net;
using NTK24.Shared;

namespace NTK24.Init.Authentication;

public class ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AuthOptions.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Api key is missing.");
            return;
        }

        var apiKey = configuration
            .GetSection(SettingsNameHelper.AuthOptionsSectionName)
            .Get<AuthOptions>()!
            .ApiKey;
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid Api Key.");
            return;
        }

        await next(context);
    }
}