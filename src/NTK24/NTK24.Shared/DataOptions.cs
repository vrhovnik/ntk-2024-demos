using System.ComponentModel.DataAnnotations;

namespace NTK24.Shared;

public class DataOptions
{
    [Required(ErrorMessage = "The connection string is required.")]
    public string ConnectionString { get; set; }
}