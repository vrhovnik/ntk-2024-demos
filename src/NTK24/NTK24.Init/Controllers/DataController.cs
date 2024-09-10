using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NTK24.Init.Helper;
using NTK24.Init.Options;
using NTK24.Interfaces;
using NTK24.Shared;

namespace NTK24.Init.Controllers;

[ApiController, Route(ConstantRouteHelper.GenerateRoute), Produces(MediaTypeNames.Application.Json)]
public class DataController(ILogger<DataController> logger,
    IOptions<InitOptions> initOptions,
    IUserService userService,
    ILinkGroupRepository linkGroupRepository,
    ILinkRepository linkRepository,
    ICategoryRepository categoryRepository) : Controller
{
    [HttpGet]
    [Route(ConstantRouteHelper.CheckRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [AllowAnonymous]
    public async Task<IActionResult> IsAliveWithConnectionCheck()
    {
        logger.LogInformation("Called alive + check endpoint at {DateCalled} - database is up and running", DateTime.UtcNow);

        var dataGenerator = new DataGenerator(initOptions, logger);
        var connIsValid = await dataGenerator.CheckDatabaseConnectionAsync(categoryRepository);

        if (connIsValid)
        {
            logger.LogInformation("Database connection is valid");
            return new ContentResult
                { StatusCode = 200, Content = $"I am alive at {DateTime.Now} with database connection opened!" };
        }

        logger.LogError("Database connection is invalid");
        return new ContentResult { StatusCode = 502, Content = $"Database connection is not working" };
    }
    
    [HttpPost]
    [Route(ConstantRouteHelper.GenerateAllRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AllAsync()
    {
        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateAllAsync(categoryRepository,
            userService,
            linkGroupRepository,
            linkRepository);
        logger.LogInformation("Users generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
        {
            StatusCode = 200,
            Content = $"Generating data for all tables was successfully"
        };
    }
    
    [HttpPost]
    [Route(ConstantRouteHelper.GenerateCategoriesRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CategoriesAsync()
    {
        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateCategoriesAsync(categoryRepository);
        logger.LogInformation("Categories generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
        {
            StatusCode = 200,
            Content = "Generating categories for table Category was successfully"
        };
    }
    
    [HttpPost]
    [Route(ConstantRouteHelper.GenerateLinkGroupsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LinkGroupsAsync()
    {
        var dataGenerator = new DataGenerator(initOptions, logger);
        logger.LogInformation("Generating link groups");
        await dataGenerator.GenerateLinkGroupsAsync(linkGroupRepository, categoryRepository, userService);
        logger.LogInformation("Link groups generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
        {
            StatusCode = 200,
            Content = "Link groups generated successfull"
        };
    }
    
    [HttpPost]
    [Route(ConstantRouteHelper.GenerateLinksRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LinksAsync()
    {
        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateLinksAsync(linkRepository, linkGroupRepository);
        logger.LogInformation("Links generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
        {
            StatusCode = 200,
            Content = "Links generated successfull"
        };
    }
    
    [HttpPost]
    [Route(ConstantRouteHelper.GenerateUsersRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UsersAsync()
    {
        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateUsersAsync(userService);
        logger.LogInformation("Users generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
        {
            StatusCode = 200,
            Content = "Users generated successfull"
        };
    }
}