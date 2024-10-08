﻿using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NTK24.Api.Authentication;
using NTK24.Api.ViewModels;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Shared;

namespace NTK24.Api.Controllers;

[ApiController, Route(ConstantRouteHelper.LinkGroupBaseRoute), Produces(MediaTypeNames.Application.Json)]
public class LinkGroupController(
    ILogger<LinkGroupController> logger,
    IOptions<AuthOptions> authOptions,
    ILinkGroupRepository linkGroupRepository) : Controller
{
    [HttpGet]
    [Route(ConstantRouteHelper.HealthRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult IsAlive()
    {
        logger.LogInformation("Called alive endpoint at {DateCalled}", DateTime.UtcNow);
        return new ContentResult { StatusCode = 200, Content = $"I am alive at {DateTime.Now}" };
    }

    [HttpGet]
    [Route(ConstantRouteHelper.GetLinkGroupsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetAllLinkGroupsAsync()
    {
        logger.LogInformation("Called getting links group at {DateCalled}", DateTime.UtcNow);
        var linkGroups = await linkGroupRepository.GetAsync();
        logger.LogInformation("Found {LinkGroupCount} link groups", linkGroups.Count);

        var list = new List<SimpleLinkGroupViewModel>();
        foreach (var linkGroup in linkGroups)
        {
            list.Add(new SimpleLinkGroupViewModel
            {
                Name = linkGroup.Name,
                CreatedAt = linkGroup.CreatedAt,
                Links = linkGroup.Links
            });
        }

        return Ok(list);
    }

    [HttpGet]
    [Route(ConstantRouteHelper.GetPublicLinkGroupsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPublicLinkGroupsAsync()
    {
        logger.LogInformation("Called getting links group at {DateCalled}", DateTime.UtcNow);
        var linkGroups = await linkGroupRepository.GetAsync();
        logger.LogInformation("Found {LinkGroupCount} link groups", linkGroups.Count);

        //if category is public, add them
        var list = (from linkGroup in linkGroups
                where linkGroup.Category.Name.ToUpper() == "PUBLIC"
                select new SimpleLinkGroupViewModel
                {
                    Name = linkGroup.Name,
                    CreatedAt = linkGroup.CreatedAt,
                    Links = linkGroup.Links
                })
            .ToList();

        return Ok(list);
    }

    [HttpGet]
    [Route(ConstantRouteHelper.GetLinkGroupsRoute + "/{linkGroupId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(IEnumerable<LinkViewModel>))]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetLinksFromLinkGroupAsync(string linkGroupId)
    {
        logger.LogInformation("Called getting links from links group at {DateCalled}", DateTime.UtcNow);
        var foundSelectLinkGroup = await linkGroupRepository.DetailsAsync(linkGroupId);
        logger.LogInformation("Found {LinksCount} links in link group {Name}", foundSelectLinkGroup.Links,
            foundSelectLinkGroup.Name);
        var links = foundSelectLinkGroup.Links.Select(link => new LinkViewModel
        {
            Name = link.Name, Url = link.Url
        });
        return Ok(links);
    }

    [HttpGet]
    [Route(ConstantRouteHelper.RedirectRoute + "/{shortName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [AllowAnonymous]
    public async Task<IActionResult> RedirectToLinkGroupAsync(string shortName)
    {
        logger.LogInformation("Called redirect endpoint at {DateCalled}", DateTime.UtcNow);
        try
        {
            var linkGroup = await linkGroupRepository.GetLinkFromShortNameAsync(shortName);
            if (linkGroup == null)
            {
                logger.LogError("Could not find link group {ShortName}", shortName);
                return new ContentResult { StatusCode = 502, Content = $"Could not find link group {shortName}" };
            }

            logger.LogInformation("Found link group {ShortName}", shortName);
            return Redirect($"{authOptions.Value.BaseUrl}/groups/details/{linkGroup.LinkGroupId}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error redirecting to link group {ShortName}", shortName);
            return new ContentResult { StatusCode = 502, Content = $"Error redirecting to link group {shortName}" };
        }
    }

    [HttpPost]
    [Route(ConstantRouteHelper.AddLinksRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> AddLinksToLinkGroupAsync([FromBody] LinkGroupViewModel linkGroupView)
    {
        logger.LogInformation("Adding links to Link Group {LinkGroupId} at {DateCalled}", linkGroupView.LinkGroupId,
            DateTime.Now);
        try
        {
            var linkGroup = await linkGroupRepository.DetailsAsync(linkGroupView.LinkGroupId);
            if (linkGroup == null)
            {
                logger.LogError("Could not find link group {LinkGroupId}", linkGroupView.LinkGroupId);
                return BadRequest();
            }

            await linkGroupRepository.AddLinksToLinkGroupAsync(linkGroupView.LinkGroupId,
                linkGroupView.Links.Select(currentLink => new Link
                {
                    LinkId = Guid.NewGuid(),
                    Group = new LinkGroup { LinkGroupId = Guid.Parse(linkGroupView.LinkGroupId) },
                    Name = currentLink.Name,
                    Url = currentLink.Url
                }).ToList());
            logger.LogInformation("Added links to Link Group {LinkGroupId}", linkGroupView.LinkGroupId);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest();
        }
    }
}