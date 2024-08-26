using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NTK24.Interfaces;
using NTK24.Models;

namespace NTK24.Web.Pages.Groups;

public class DetailsPageModel(ILogger<DetailsPageModel> logger, ILinkGroupRepository linkGroupRepository) : PageModel
{
    public async Task OnGetAsync(string linkGroupId)
    {
        logger.LogInformation("Loading details page for orders {DateCreated}", DateTime.Now);
        LinkGroup = await linkGroupRepository.DetailsAsync(linkGroupId);
        logger.LogInformation("Received details for group {LinkGroupId}", linkGroupId);
    }

    [BindProperty]
    public LinkGroup LinkGroup { get; set; }
}