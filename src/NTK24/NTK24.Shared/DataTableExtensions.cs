using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace NTK24.Shared;

public static class DataTableExtensions
{
    public static async Task<bool> WriteBulkToDatabaseAsync(this SqlConnection connection, DataTable dt)
    {
        try
        {
            using var tagBulkInsert = new SqlBulkCopy(connection);
            tagBulkInsert.DestinationTableName = dt.TableName;
            await tagBulkInsert.WriteToServerAsync(dt);
        }
        catch (Exception tagException)
        {
            Debug.WriteLine(tagException.Message);
            return false;
        }

        return true;
    }

    
    public static DataTable ToDataTable<T>(this IList<T> data)
    {
        var properties = 
            TypeDescriptor.GetProperties(typeof(T));
        var table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        foreach (var item in data)
        {
            var row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            table.Rows.Add(row);
        }
        return table;
    }
}