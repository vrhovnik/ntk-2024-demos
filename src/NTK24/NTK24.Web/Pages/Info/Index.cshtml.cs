using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NTK24.Web.Pages.Info;

[AllowAnonymous]
public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public void OnGet()
    {
        logger.LogInformation("Index page visited at {DateLoaded}", DateTime.UtcNow);
        //information about on which machine the page was loaded
        logger.LogInformation("Machine name: {MachineName}", Environment.MachineName);
        Message = Environment.MachineName;
    }

    [BindProperty] public string Message { get; set; } = "";
}