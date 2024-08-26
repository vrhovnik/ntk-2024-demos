using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Web.Models;

namespace NTK24.Web.Pages.Groups;

public class IndexPageModel(ILogger<IndexPageModel> logger, ILinkGroupRepository linkGroupRepository) : PageModel
{
    public void OnGet() => logger.LogInformation("Loading index page for link groups {DateCreated}", DateTime.Now);

    public async Task<IActionResult> OnGetFilterAsync(string query)
    {
        logger.LogInformation("Getting groups for page 1 with query {Query}", query);
        var groups = await linkGroupRepository.SearchAsync(1, 20, query);
        logger.LogInformation("Received {Count} groups for page 1 with query {Query}", groups.Count, query);
        var data = groups.Select(currentData => new LinkGroupViewModel(
            currentData.LinkGroupId.ToString(),
            currentData.Name,
            currentData.ShortName,
            currentData.Clicked,
            currentData.Links.Count,
            currentData.CreatedAt.ToString("dd.MM.yyyy")));
        return new JsonResult(data);
    }
}