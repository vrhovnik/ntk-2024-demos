using NTK24.Web.Models;

namespace NTK24.Web.Base;

public interface IUserDataContext
{
    UserViewModel GetCurrentUser();
    Task LogOut();
}