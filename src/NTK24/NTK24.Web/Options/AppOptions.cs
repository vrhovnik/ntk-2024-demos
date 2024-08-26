using System.ComponentModel.DataAnnotations;

namespace NTK24.Web.Options;

public class AppOptions
{
    [Required(ErrorMessage = "ApiUrl is required")]
    public string ApiUrl { get; set; }
}