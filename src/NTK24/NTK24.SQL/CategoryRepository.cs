using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Shared;

namespace NTK24.SQL
{
    public class CategoryRepository(string connectionString)
        : BaseRepository<Category>(connectionString), ICategoryRepository
    {
        public override async Task<bool> BulkInsertAsync(IEnumerable<Category> entites)
        {
            var dtCategories = new DataTable();
            dtCategories.TableName = "Categories";
            dtCategories.Columns.Add("CategoryId", typeof(Guid));
            dtCategories.Columns.Add("Name", typeof(string));
            foreach (var category in entites)
            {
                var row = dtCategories.NewRow();
                row["CategoryId"] = category.CategoryId;
                row["Name"] = category.Name;
                dtCategories.Rows.Add(row);
            }

            await using var connection = new SqlConnection(connectionString);
            var isSuccess = await connection.WriteBulkToDatabaseAsync(dtCategories);
            return isSuccess;
        }

        public override async Task<Category> DetailsAsync(string entityId)
        {
            await using var connection = new SqlConnection(connectionString);
            var foundCategory = await connection.QuerySingleOrDefaultAsync<Category>(
                "SELECT C.CategoryId, C.Name FROM Category C WHERE C.CategoryId=@entityId", new {entityId});
            return foundCategory;
        }

        public override async Task<List<Category>> GetAsync()
        {
            await using var connection = new SqlConnection(connectionString);
            var categories = await connection.QueryAsync<Category>(
                "SELECT C.CategoryId, C.Name FROM Categories C");
            return categories.ToList();
        }
    }
}