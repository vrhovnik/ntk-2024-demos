using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using NTK24.Models;

namespace NTK24.Web.Base;

public static class UserDataContextExtensions
{
    public static ClaimsPrincipal GenerateClaims(this SulUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}