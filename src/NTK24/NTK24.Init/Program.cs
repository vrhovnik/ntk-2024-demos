using System.Net;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NTK24.Init.Helper;
using NTK24.Init.Options;
using NTK24.Init.Services;
using NTK24.Interfaces;
using NTK24.Shared;
using NTK24.SQL;
using NTK24.Web.Base;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<InitOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.DataOptionsSectionName));
builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AuthOptionsSectionName));
builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.StorageOptionsSectionName));

var sqlOptions = builder.Configuration.GetSection(SettingsNameHelper.DataOptionsSectionName).Get<InitOptions>()!;
var sqlConnectionString = sqlOptions.ConnectionString;
sqlConnectionString += $"Initial Catalog={sqlOptions.DatabaseName};";
builder.Services.AddScoped<IUserService, SulUserService>(_ => new SulUserService(sqlConnectionString));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>(_ => new CategoryRepository(sqlConnectionString));
builder.Services.AddScoped<ILinkGroupRepository, LinkGroupRepository>(_ =>
    new LinkGroupRepository(sqlConnectionString));
builder.Services.AddScoped<ILinkRepository, LinkRepository>(_ => new LinkRepository(sqlConnectionString));
builder.Services.AddScoped<IDatabaseGenerator, SqlDatabaseGenerator>(_ =>
    new SqlDatabaseGenerator(sqlOptions.ConnectionString));
builder.Services.AddScoped<IUserDataContext, UserDataContext>();

var azureStorageOptions = builder.Configuration.GetSection(SettingsNameHelper.StorageOptionsSectionName)
    .Get<StorageOptions>()!;
builder.Services.AddScoped<IScriptDownloader, StorageScriptDownloader>(_ =>
    new StorageScriptDownloader(azureStorageOptions.TableScriptContainer, azureStorageOptions.ConnectionString));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/generate").MapGenerateApi().WithTags("generate");
app.MapGroup("/init").MapInitApi().WithTags("init");

app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        if (exception != null)
        {
            var message = $"{exception.Error.Message}";
            await context.Response.WriteAsync(message).ConfigureAwait(false);
        }
    });
});

app.MapHealthChecks($"/{ConstantRouteHelper.HealthRoute}", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.Run();