using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
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

    public virtual async Task<bool> BulkInsertAsync(IEnumerable<TEntity> entites)
    {
        try
        {
            var dt = entites.ToList().ToDataTable();
            await using var connection = new SqlConnection(connectionString);
            connection.Open();
            await connection.WriteBulkToDatabaseAsync(dt);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> IsConnectionToDatabaseValidAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        var checkResult = false;
        try
        {
            await connection.OpenAsync();
            checkResult = true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return checkResult;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }

        return checkResult;
    }
}