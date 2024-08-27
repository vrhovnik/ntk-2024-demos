using NTK24.Interfaces;
using NTK24.Shared;

namespace NTK24.SQL;

public abstract class BaseRepository<TEntity>(string connectionString) : IDataRepository<TEntity>
    where TEntity : class
{
    public virtual Task<PaginatedList<TEntity>> SearchAsync(int page, int pageSize, string query = "") =>
        throw new NotImplementedException();

    public virtual async Task<List<TEntity>> SearchAsync(string query = "") => await SearchAsync(1, 100, query);
    public virtual Task<PaginatedList<TEntity>> GetAsync(int page, int pageSize) => 
        throw new NotImplementedException();
    public virtual Task<List<TEntity>> GetAsync() => throw new NotImplementedException();
    public virtual Task<bool> DeleteAsync(string entityId) => throw new NotImplementedException();
    public virtual Task<bool> UpdateAsync(TEntity entity) => throw new NotImplementedException();
    public virtual Task<TEntity> InsertAsync(TEntity entity) => throw new NotImplementedException();
    public virtual Task<TEntity> DetailsAsync(string entityId) => throw new NotImplementedException();
    public virtual Task<bool> BulkInsertAsync(IEnumerable<TEntity> entites)=> throw new NotImplementedException();
}