using Microsoft.Extensions.Options;
using NTK24.Shared;
using NTK24.Web.Options;

namespace NTK24.Web.Services;

public class GenerateLinkGroupService(
    HttpClient client,
    IOptions<AppOptions> appOptions,
    IOptions<AuthOptions> authOptions,
    ILogger<GenerateLinkGroupService> logger)
{
    public async Task<bool> GenerateRandomLinkGroupAsync()
    {
        logger.LogInformation("Creating random post data for link groups at {DateCreated}", DateTime.Now);
        client.BaseAddress = new Uri(appOptions.Value.InitUrl, UriKind.RelativeOrAbsolute);
        client.DefaultRequestHeaders.Add(AuthOptions.ApiKeyHeaderName, authOptions.Value.ApiKey);
        try
        {
            var response =
                await client.SendAsync(new HttpRequestMessage(HttpMethod.Post,
                    $"{ConstantRouteHelper.GenerateRoute}/{ConstantRouteHelper.GenerateLinkGroupsRoute}"));
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            logger.LogError("Error while generating random post data for link groups: {Error}", e.Message);
            return false;
        }
    }
}