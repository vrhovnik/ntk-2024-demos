using NTK24.Models;

namespace NTK24.Api.ViewModels;

public class SimpleLinkGroupViewModel
{
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Link> Links { get; set; } = new();
}