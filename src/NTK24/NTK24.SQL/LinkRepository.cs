using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Shared;

namespace NTK24.SQL;

public class LinkRepository(string connectionString)
    : BaseRepository<Link>(connectionString), ILinkRepository
{
    public override async Task<bool> BulkInsertAsync(IEnumerable<Link> entites)
    {
        var dtLinks = new DataTable
        {
            TableName = "Links"
        };
        dtLinks.Columns.Add("LinkId", typeof(Guid));
        dtLinks.Columns.Add("Name", typeof(string));
        dtLinks.Columns.Add("Url", typeof(string));
        dtLinks.Columns.Add("LinkGroupId", typeof(string));
       
        foreach (var link in entites)
        {
            var row = dtLinks.NewRow();
            row["LinkGroupId"] = link.LinkId;
            row["Name"] = link.Name;
            row["Url"] = link.Url;
            row["LinkGroupId"] = link.Group.LinkGroupId;
            dtLinks.Rows.Add(row);
        }

        await using var connection = new SqlConnection(connectionString);
        var isSuccess = await connection.WriteBulkToDatabaseAsync(dtLinks);
        return isSuccess;
    }

    public override async Task<PaginatedList<Link>> SearchAsync(int page, int pageSize, string query = "")
    {
        await using var connection = new SqlConnection(connectionString);
        var sql = "SELECT L.LinkId, L.Name, L.Url, G.LinkGroupId, G.Name, G.Description, G.ShortName, " +
                  "G.UserId,G.Clicked,G.CategoryId,G.CreatedAt FROM Links L " +
                  "JOIN LinkGroups G ON G.LinkGroupId = L.LinkId ";

        if (!string.IsNullOrEmpty(query))
            sql +=
                $"WHERE G.Name LIKE '%{query}%' OR G.Description LIKE '%{query}%' OR G.ShortName LIKE '%{query}%' OR L.Name LIKE '%{query}%'";

        var grid = await connection.QueryMultipleAsync(sql);
        var lookup = new Dictionary<Guid, Link>();
        grid.Read<Link, LinkGroup, Link>((link, linkGroup) =>
        {
            link.Group = linkGroup;
            if (!lookup.TryGetValue(link.LinkId, out _))
                lookup.Add(link.LinkId, link);
            return link;
        }, splitOn: "LinkGroupId");
        
        return PaginatedList<Link>.Create(lookup.Values.AsQueryable(), page, pageSize, query);
    }
}