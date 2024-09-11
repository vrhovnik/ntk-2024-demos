using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Web.Base;
using NTK24.Web.Services;

namespace NTK24.Web.Pages.User;

[Authorize]
public class DashboardPageModel(
    ILogger<DashboardPageModel> logger,
    IUserDataContext userDataContext,
    GenerateLinkGroupService generateLinkGroupService,
    ILinkGroupRepository linkGroupRepository) : BasePageModel
{
    public async Task OnGetAsync()
    {
        var userViewModel = userDataContext.GetCurrentUser();
        logger.LogInformation("Loading dashboard for user {User} - starting at {DateStart}", userViewModel.Fullname,
            DateTime.Now);
        MyLinkGroups = await linkGroupRepository.GetFromSpecificUserAsync(userViewModel.UserId.ToString());
        logger.LogInformation("Loading dashboard for user {User} - finished at {DateEnd} - with {LinkGroupCount}",
            userViewModel.Fullname,
            DateTime.Now, MyLinkGroups.Count);
    }

    public async Task<RedirectToPageResult> OnPostAsync()
    {
        logger.LogInformation("Creating random post data at {DateCreated}", DateTime.Now);

        var isSuccess = await generateLinkGroupService.GenerateRandomLinkGroupAsync();
        logger.LogInformation(isSuccess
            ? "Generated random post data, redirecting to dashboard to reload"
            : "Error while generating random post data, redirecting to dashboard to reload. Check logs.");

        return RedirectToPage();
    }

    [BindProperty] public List<LinkGroup> MyLinkGroups { get; set; } = new();
}