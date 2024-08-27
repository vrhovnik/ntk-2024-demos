namespace NTK24.Interfaces;

public interface IScriptDownloader
{
    Task<string> GetScriptAsync(string name);
}