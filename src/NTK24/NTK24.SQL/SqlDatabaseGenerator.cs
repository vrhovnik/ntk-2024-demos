using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using NTK24.Interfaces;

namespace NTK24.SQL;

public class SqlDatabaseGenerator(string connectionString) : IDatabaseGenerator
{
    public async Task<bool> IsCreatedAsync(string databaseName)
    {
        var sqlConnection = new SqlConnection(connectionString);
        var dbExistsCount =
            await sqlConnection.QuerySingleOrDefaultAsync<int>(
                $"SELECT count(*) FROM master.dbo.sysdatabases WHERE name = '{databaseName}'");
        return dbExistsCount > 0;
    }

    public async Task<bool> GenerateAsync(string databaseName)
    {
        var sqlConnection = new SqlConnection(connectionString);
        try
        {
            await sqlConnection.ExecuteAsync(
                $"CREATE DATABASE {databaseName} collate SQL_Latin1_General_CP1_CI_AS");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> GenerateTablesAsync(string tableScript)
    {
        var sqlConnection = new SqlConnection(connectionString);
        try
        {
            await sqlConnection.ExecuteAsync(tableScript);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }
}