using System.Net;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NTK24.Interfaces;
using NTK24.Shared;
using NTK24.SQL;
using NTK24.Web.Base;
using NTK24.Web.Options;
using NTK24.Web.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AuthOptionsSectionName));
builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AppOptionsSectionName));
builder.Services.AddOptions<DataOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.DataOptionsSectionName));

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

var sqlOptions = builder.Configuration.GetSection(SettingsNameHelper.DataOptionsSectionName).Get<DataOptions>();
var sqlConnectionString = sqlOptions!.ConnectionString;
builder.Services.AddScoped<IUserService, SulUserService>(_ => new SulUserService(sqlConnectionString));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>(_ => new CategoryRepository(sqlConnectionString));
builder.Services.AddScoped<ILinkGroupRepository, LinkGroupRepository>(_ => new LinkGroupRepository(sqlConnectionString));
builder.Services.AddScoped<ILinkRepository, LinkRepository>(_ => new LinkRepository(sqlConnectionString));
builder.Services.AddScoped<IUserDataContext, UserDataContext>();
builder.Services.AddHttpClient<GenerateLinkGroupService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = new PathString("/User/Login"));
builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
    options.Conventions.AddPageRoute("/Info/Index", ""));
var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
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
}).AllowAnonymous();

app.Run();