using System.ComponentModel.DataAnnotations;

namespace NTK24.Init.Options;

public class StorageOptions
{
    [Required(ErrorMessage = "The connection string is required.")]
    public required string ConnectionString { get; set; }
    [Required(ErrorMessage = "The table container is required.")]
    public required string TableScriptContainer { get; set; }
}