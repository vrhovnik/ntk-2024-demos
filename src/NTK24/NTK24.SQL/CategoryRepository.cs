using Dapper;
using Microsoft.Data.SqlClient;
using NTK24.Interfaces;
using NTK24.Models;

namespace NTK24.SQL
{
    public class CategoryRepository(string connectionString)
        : BaseRepository<Category>(connectionString), ICategoryRepository
    {
        public override async Task<Category> DetailsAsync(string entityId)
        {
            await using var connection = new SqlConnection(connectionString);
            var foundCategory = await connection.QuerySingleOrDefaultAsync<Category>(
                "SELECT C.CategoryId, C.Name FROM Category C WHERE C.CategoryId=@entityId", new {entityId});
            return foundCategory;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            await using var connection = new SqlConnection(connectionString);
            var categories = await connection.QueryAsync<Category>(
                "SELECT C.CategoryId, C.Name FROM Categories C");
            return categories.ToList();
        }

        public override async Task<List<Category>> GetAsync() => await GetAllAsync();
    }
}