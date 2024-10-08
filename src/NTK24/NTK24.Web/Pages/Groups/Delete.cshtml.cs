﻿using Microsoft.AspNetCore.Mvc;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Web.Base;

namespace NTK24.Web.Pages.Groups;

public class DeletePageModel(ILogger<DeletePageModel> logger, ILinkGroupRepository linkGroupRepository) : BasePageModel
{
    public async Task OnGetAsync()
    {
        logger.LogInformation("Called Delete endpoint at {DateCalled} from id {LinkGroupId}", DateTime.UtcNow,
            LinkGroupId);
        DeleteLinkGroup = await linkGroupRepository.DetailsAsync(LinkGroupId);
        logger.LogInformation("Found {LinkCount} links for group {LinkGroupId}", DeleteLinkGroup.Links.Count,
            LinkGroupId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LinkGroupId = DeleteLinkGroup.LinkGroupId.ToString();
        logger.LogInformation("Called Delete endpoint at {DateCalled} from id {LinkGroupId}", DateTime.UtcNow,
            LinkGroupId);
        try
        {
            await linkGroupRepository.DeleteAsync(LinkGroupId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting group {LinkGroupId}", LinkGroupId);
            Message = $"Error deleting group with {LinkGroupId} - please try again later";
        }
        return RedirectToPage("/User/Dashboard");
    }

    [BindProperty(SupportsGet = true)] public required string LinkGroupId { get; set; }
    [BindProperty] public LinkGroup DeleteLinkGroup { get; set; }
}