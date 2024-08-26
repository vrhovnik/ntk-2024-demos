using System.ComponentModel.DataAnnotations;

namespace NTK24.Web.Models;

public class LoginPageViewModel
{
    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}