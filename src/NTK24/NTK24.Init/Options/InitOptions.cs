using NTK24.Shared;

namespace NTK24.Init.Options;

public class InitOptions : DataOptions
{
    public int RecordCount { get; set; } = 500;
    public string DefaultPassword { get; set; } = "password";
    public string DatabaseName { get; set; } = "SulDb";
    public string TableScriptName { get; set; } = "suldbtables.sql";
}