namespace NTK24.Web.Models;

public class UserViewModel
{
    public Guid UserId { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}