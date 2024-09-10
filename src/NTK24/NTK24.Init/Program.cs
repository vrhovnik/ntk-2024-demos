using System.Net;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using NTK24.Init.Authentication;
using NTK24.Init.Options;
using NTK24.Init.Services;
using NTK24.Interfaces;
using NTK24.Shared;
using NTK24.SQL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(conf =>
{
    conf.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Api key to access the NTK api's",
        Type = SecuritySchemeType.ApiKey,
        Name = AuthOptions.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };
    conf.AddSecurityRequirement(requirement);
});
builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

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

var azureStorageOptions = builder.Configuration.GetSection(SettingsNameHelper.StorageOptionsSectionName)
    .Get<StorageOptions>()!;
builder.Services.AddScoped<IScriptDownloader, StorageScriptDownloader>(_ =>
    new StorageScriptDownloader(azureStorageOptions.TableScriptContainer, azureStorageOptions.ConnectionString));

builder.Services.AddScoped<ApiKeyAuthFilter>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//minimal api implementation
// app.MapGroup("/generate").MapGenerateApi().WithTags("generate");
// app.MapGroup("/init").MapInitApi().WithTags("init");
app.UseRouting();
app.MapControllers();
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