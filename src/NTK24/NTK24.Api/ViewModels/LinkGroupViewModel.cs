namespace NTK24.Api.ViewModels;

public class LinkGroupViewModel
{
    public string LinkGroupId { get; set; }
    public List<LinkViewModel> Links { get; set; } = new();
}