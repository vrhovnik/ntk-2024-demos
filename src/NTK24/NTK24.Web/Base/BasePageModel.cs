using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NTK24.Web.Base;

public abstract class BasePageModel : PageModel
{
    [TempData]
    public string? Message { get; set; }
}