using NTK24.Shared;

namespace NTK24.Interfaces;

public interface IDataRepository<T> where T : class
{
    Task<PaginatedList<T>> SearchAsync(int page, int pageSize, string query = "");
    Task<List<T>> SearchAsync(string query = "");
    Task<List<T>> GetAsync();
    Task<bool> DeleteAsync(string entityId);
    Task<bool> UpdateAsync(T entity);
    Task<T> InsertAsync(T entity);
    Task<T> DetailsAsync(string entityId);
    Task<bool> BulkInsertAsync(IEnumerable<T> entites);
    Task<bool> IsConnectionToDatabaseValidAsync();
}