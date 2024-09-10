using Bogus;
using Microsoft.Extensions.Options;
using NTK24.Init.Options;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Shared;

namespace NTK24.Init.Helper;

public class DataGenerator(IOptions<InitOptions> initOptions, ILogger logger)
{
    public async Task<bool> CheckDatabaseConnectionAsync<T>(IDataRepository<T> dataRepository) 
        where T : class =>
        await dataRepository.IsConnectionToDatabaseValidAsync();

    public async Task GenerateAllAsync(ICategoryRepository categoryRepository,
        IUserService userService, ILinkGroupRepository linkGroupRepository, ILinkRepository linkRepository)
    {
        logger.LogInformation("Starting generating categories");
        await GenerateCategoriesAsync(categoryRepository);
        logger.LogInformation("Done generating categories, going to generate users");
        await GenerateUsersAsync(userService);
        logger.LogInformation("Done generating users, going to generate link groups");
        await GenerateLinkGroupsAsync(linkGroupRepository, categoryRepository, userService);
        logger.LogInformation("Done generating link groups, going to generate links and add them to link groups");
        await GenerateLinksAsync(linkRepository, linkGroupRepository);
    }

    public async Task GenerateCategoriesAsync(ICategoryRepository categoryRepository)
    {
        var categories = new Faker<Category>()
            .RuleFor(category => category.CategoryId, (f, _) => f.Random.Guid())
            .RuleFor(category => category.Name, (f, _) => f.Commerce.Product())
            .GenerateLazy(initOptions.Value.RecordCount);

        await categoryRepository.BulkInsertAsync(categories);
    }

    public async Task GenerateUsersAsync(IUserService userService)
    {
        var passwdHash = PasswordHash.CreateHash(initOptions.Value.DefaultPassword);
        var categories = new Faker<SulUser>()
            .RuleFor(category => category.UserId, (f, _) => f.Random.Guid())
            .RuleFor(category => category.FullName, (f, _) => f.Name.FullName())
            .RuleFor(category => category.Email, (f, _) => f.Internet.Email())
            .RuleFor(u => u.Password, (_, _) => passwdHash)
            .GenerateLazy(initOptions.Value.RecordCount);

        await userService.BulkInsertAsync(categories);
    }

    public async Task GenerateLinksAsync(ILinkRepository linkRepository, ILinkGroupRepository linkGroupRepository)
    {
        var linkGroups = await linkGroupRepository.GetAsync();
        var links = new Faker<Link>()
            .RuleFor(link => link.LinkId, (f, _) => f.Random.Guid())
            .RuleFor(link => link.Name, (f, _) => f.Commerce.ProductName())
            .RuleFor(link => link.Url, (f, _) => f.Internet.Url())
            .RuleFor(link => link.Group, (f, _) => f.PickRandom(linkGroups))
            .GenerateLazy(initOptions.Value.RecordCount);

        await linkRepository.BulkInsertAsync(links);
    }

    public async Task GenerateLinkGroupsAsync(ILinkGroupRepository linkGroupRepository,
        ICategoryRepository categoryRepository, IUserService userService)
    {
        var categories = await categoryRepository.GetAsync();
        var users = await userService.GetAsync();
        var linkGroups = new Faker<LinkGroup>()
            .RuleFor(linkGroup => linkGroup.LinkGroupId, (f, _) => f.Random.Guid())
            .RuleFor(linkGroup => linkGroup.Name, (f, _) => f.Commerce.ProductAdjective())
            .RuleFor(linkGroup => linkGroup.Description, (f, _) => f.Commerce.ProductDescription())
            .RuleFor(linkGroup => linkGroup.ShortName, (f, _) => f.Commerce.ProductMaterial())
            .RuleFor(linkGroup => linkGroup.User, (f, _) => f.PickRandom(users))
            .RuleFor(linkGroup => linkGroup.Category, (f, _) => f.PickRandom(categories))
            .RuleFor(linkGroup => linkGroup.CreatedAt, (f, _) => f.Date.Past())
            .GenerateLazy(initOptions.Value.RecordCount);

        await linkGroupRepository.BulkInsertAsync(linkGroups);
    }
}