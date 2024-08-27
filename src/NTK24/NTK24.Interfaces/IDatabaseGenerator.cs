namespace NTK24.Interfaces;

public interface IDatabaseGenerator
{
    Task<bool> IsCreatedAsync(string databaseName);
    Task<bool> GenerateAsync(string databaseName);
    Task<bool> GenerateTablesAsync(string tableScript);
}