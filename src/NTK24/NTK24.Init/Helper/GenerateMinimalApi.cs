using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NTK24.Init.Options;
using NTK24.Interfaces;
using NTK24.Shared;

namespace NTK24.Init.Helper;

public static class GenerateMinimalApi
{
    public static RouteGroupBuilder MapInitApi(this RouteGroupBuilder group)
    {
        group.MapPost("db", GenerateDatabaseAsync);
        group.MapPost("tables", GenerateTablesAsync);
        return group;
    }

    private static async Task<IResult> GenerateTablesAsync(
        [FromHeader(Name = AuthOptions.ApiKeyHeaderName)]
        string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        IDatabaseGenerator databaseGenerator,
        IScriptDownloader scriptDownloader,
        ILogger logger)
    {
        var databaseName = initOptions.Value.DatabaseName;
        logger.LogInformation("Database {DatabaseName} generation started at {DateLoaded}", databaseName, DateTime.Now);
        if (!IsApiKeyValid(apiKey, authOptions, logger))
            return Results.BadRequest("Invalid API Key, need to be have access to perform admin actions");

        logger.LogInformation("Check if database {DatabaseName} already created", databaseName);
        var isDatabaseCreated = await databaseGenerator.IsCreatedAsync(databaseName);
        if (!isDatabaseCreated)
        {
            logger.LogInformation("Database not created, generating database {DatabaseName} from connecting string {ConnectionString}",
                databaseName, initOptions.Value.ConnectionString);
            await GenerateDatabaseAsync(apiKey, authOptions, initOptions, databaseGenerator, logger);
        }
        logger.LogInformation("Downloading script from {ScriptUrl}", initOptions.Value.TableScriptName);
        var script = await scriptDownloader.GetScriptAsync(initOptions.Value.TableScriptName);
        logger.LogInformation("Script downloaded successfully: {Script}", script);
        await databaseGenerator.GenerateTablesAsync(script);
        logger.LogInformation("Database tables generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok($"Generating database {databaseName} tables was successfully");
    }

    private static async Task<IResult> GenerateDatabaseAsync(
        [FromHeader(Name = AuthOptions.ApiKeyHeaderName)]
        string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        IDatabaseGenerator databaseGenerator,
        ILogger logger)
    {
        var databaseName = initOptions.Value.DatabaseName;
        logger.LogInformation("Database {DatabaseName} generation started at {DateLoaded}", databaseName, DateTime.Now);
        if (!IsApiKeyValid(apiKey, authOptions, logger))
            return Results.BadRequest("Invalid API Key, need to be have access to perform admin actions");

        logger.LogInformation("Check if database {DatabaseName} already created", databaseName);
        var isDatabaseCreated = await databaseGenerator.IsCreatedAsync(databaseName);
        if (isDatabaseCreated)
        {
            logger.LogInformation("Database already created");
            return Results.Ok($"Database {databaseName} already created");
        }

        logger.LogInformation(
            "Database not created, start generating database {DatabaseName} from connecting string {ConnectionString}",
            databaseName, initOptions.Value.ConnectionString);
        logger.LogInformation("Database already created, start generating tables for database {DatabaseName}", databaseName);
        await databaseGenerator.GenerateAsync(databaseName);
        logger.LogInformation("Database generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok($"Generating database {databaseName} was successfully");
    }

    public static RouteGroupBuilder MapGenerateApi(this RouteGroupBuilder group)
    {
        group.MapPost("all", AddAllApiAsync);
        group.MapPost("categories", AddCategoriesApiAsync);
        group.MapPost("users", AddUsersApiAsync);
        group.MapPost("link-groups", AddLinkGroupsApiAsync);
        group.MapPost("links", AddLinksApiAsync);
        return group;
    }

    private static async Task<IResult> AddAllApiAsync([FromHeader(Name = AuthOptions.ApiKeyHeaderName)] string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        ICategoryRepository categoryRepository,
        IUserService userService,
        ILinkGroupRepository linkGroupRepository,
        ILinkRepository linkRepository,
        ILogger logger)
    {
        if (!IsApiKeyValid(apiKey, authOptions, logger)) return Results.BadRequest("Invalid API Key");

        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateAllAsync(categoryRepository,
            userService,
            linkGroupRepository,
            linkRepository);
        logger.LogInformation("Data generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok("All data generated successfully");
    }

    private static async Task<IResult> AddCategoriesApiAsync(
        [FromHeader(Name = AuthOptions.ApiKeyHeaderName)]
        string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        ICategoryRepository categoryRepository,
        ILogger logger)
    {
        if (!IsApiKeyValid(apiKey, authOptions, logger)) return Results.BadRequest("Invalid API Key");

        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateCategoriesAsync(categoryRepository);
        logger.LogInformation("Categories generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok("Categories generated successfully");
    }

    private static async Task<IResult> AddLinkGroupsApiAsync(
        [FromHeader(Name = AuthOptions.ApiKeyHeaderName)]
        string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        ILinkGroupRepository linkGroupRepository,
        ICategoryRepository categoryRepository,
        ILogger logger)
    {
        if (!IsApiKeyValid(apiKey, authOptions, logger)) return Results.BadRequest("Invalid API Key");

        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateLinkGroupsAsync(linkGroupRepository, categoryRepository);
        logger.LogInformation("Link groups generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok("Link groups generated successfully");
    }

    private static async Task<IResult> AddLinksApiAsync([FromHeader(Name = AuthOptions.ApiKeyHeaderName)] string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        ILinkGroupRepository linkGroupRepository,
        ILinkRepository linkRepository,
        ILogger logger)
    {
        if (!IsApiKeyValid(apiKey, authOptions, logger)) return Results.BadRequest("Invalid API Key");

        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateLinksAsync(linkRepository, linkGroupRepository);
        logger.LogInformation("Links generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok("Links generated successfully");
    }

    private static async Task<IResult> AddUsersApiAsync([FromHeader(Name = AuthOptions.ApiKeyHeaderName)] string apiKey,
        IOptions<AuthOptions> authOptions,
        IOptions<InitOptions> initOptions,
        IUserService userService,
        ILogger logger)
    {
        if (!IsApiKeyValid(apiKey, authOptions, logger)) return Results.BadRequest("Invalid API Key");

        var dataGenerator = new DataGenerator(initOptions, logger);
        await dataGenerator.GenerateUsersAsync(userService);
        logger.LogInformation("Users generated successfully at {DateLoaded}", DateTime.Now);
        return Results.Ok("Users generated successfully");
    }

    private static bool IsApiKeyValid(string apiKey, IOptions<AuthOptions> authOptions, ILogger logger)
    {
        logger.LogInformation("Check API key");
        if (apiKey != authOptions.Value.ApiKey)
        {
            logger.LogInformation("Invalid API key");
            return false;
        }

        logger.LogInformation("API key is the following: {ApiKey}", apiKey);
        return true;
    }
}