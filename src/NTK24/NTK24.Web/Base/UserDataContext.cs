using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using NTK24.Web.Models;

namespace NTK24.Web.Base;

public class UserDataContext(IHttpContextAccessor httpContextAccessor) : IUserDataContext
{
    public UserViewModel GetCurrentUser()
    {
        var httpContextUser = httpContextAccessor.HttpContext?.User;
        ArgumentNullException.ThrowIfNull(httpContextUser, nameof(httpContextUser));
        
        var currentUser = new UserViewModel();
        var claimName = httpContextUser.FindFirst(ClaimTypes.Name);
        currentUser.Fullname = claimName!.Value;

        var claimId = httpContextUser.FindFirst(ClaimTypes.NameIdentifier);
        currentUser.UserId = Guid.Parse(claimId!.Value);

        var claimEmail = httpContextUser.FindFirst(ClaimTypes.Email);
        currentUser.Email = claimEmail!.Value;

        return currentUser;
    }

    public Task LogOut() => httpContextAccessor.HttpContext!.SignOutAsync();
}

