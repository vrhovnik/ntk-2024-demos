using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NTK24.Init.Authentication;
using NTK24.Init.Helper;
using NTK24.Init.Options;
using NTK24.Interfaces;
using NTK24.Shared;

namespace NTK24.Init.Controllers;

[ApiController, Route(ConstantRouteHelper.DatabaseRoute), Produces(MediaTypeNames.Application.Json)]
public class DatabaseController(
    ILogger<DatabaseController> logger,
    IOptions<InitOptions> initOptions,
    ICategoryRepository categoryRepository,
    IDatabaseGenerator databaseGenerator,
    IScriptDownloader scriptDownloader) : Controller
{
    [HttpPost]
    [Route(ConstantRouteHelper.CreateTablesRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GenerateTablesAsync()
    {
        var databaseName = initOptions.Value.DatabaseName;
        logger.LogInformation("Database {DatabaseName} generation started at {DateLoaded}", databaseName, DateTime.Now);
        logger.LogInformation("Check if database {DatabaseName} already created", databaseName);
        var isDatabaseCreated = await databaseGenerator.IsCreatedAsync(databaseName);
        if (!isDatabaseCreated)
        {
            logger.LogInformation(
                "Database not created, generating database {DatabaseName} from connecting string {ConnectionString}",
                databaseName, initOptions.Value.ConnectionString);
            await GenerateDatabaseAsync();
        }

        logger.LogInformation("Downloading script from {ScriptUrl}", initOptions.Value.TableScriptName);
        var script = await scriptDownloader.GetScriptAsync(initOptions.Value.TableScriptName);
        logger.LogInformation("Script downloaded successfully: {Script}", script);
        await databaseGenerator.GenerateTablesAsync(script);
        logger.LogInformation("Database tables generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
        {
            StatusCode = 200,
            Content = $"Generating database {databaseName} tables was successfully"
        };
    }

    [HttpPost]
    [Route(ConstantRouteHelper.CreateDbRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GenerateDatabaseAsync()
    {
        var databaseName = initOptions.Value.DatabaseName;
        logger.LogInformation("Database {DatabaseName} generation started at {DateLoaded}", databaseName, DateTime.Now);
        logger.LogInformation("Check if database {DatabaseName} already created", databaseName);
        var isDatabaseCreated = await databaseGenerator.IsCreatedAsync(databaseName);
        if (isDatabaseCreated)
        {
            logger.LogInformation("Database already created");
            return new ContentResult
                { StatusCode = 409, Content = $"Database {databaseName} already created" };
        }

        logger.LogInformation(
            "Database not created, start generating database {DatabaseName} from connecting string {ConnectionString}",
            databaseName, initOptions.Value.ConnectionString);
        logger.LogInformation("Database already created, start generating tables for database {DatabaseName}",
            databaseName);
        await databaseGenerator.GenerateAsync(databaseName);
        logger.LogInformation("Database generated successfully at {DateLoaded}", DateTime.Now);
        return new ContentResult
            { StatusCode = 200, Content = $"Database {databaseName} created" };
    }

    [HttpGet]
    [Route(ConstantRouteHelper.CheckRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [AllowAnonymous]
    public async Task<IActionResult> IsAliveWithConnectionCheck()
    {
        logger.LogInformation("Called alive + check endpoint at {DateCalled}", DateTime.UtcNow);

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
}