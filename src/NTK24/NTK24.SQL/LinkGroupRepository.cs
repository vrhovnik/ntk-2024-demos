﻿using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Shared;

namespace NTK24.SQL;

public class LinkGroupRepository(string connectionString)
    : BaseRepository<LinkGroup>(connectionString), ILinkGroupRepository
{
    public override async Task<bool> BulkInsertAsync(IEnumerable<LinkGroup> entites)
    {
        var dtGroups = new DataTable();
        dtGroups.TableName = "LinkGroups";
        dtGroups.Columns.Add("LinkGroupId", typeof(Guid));
        dtGroups.Columns.Add("Name", typeof(string));
        dtGroups.Columns.Add("Description", typeof(string));
        dtGroups.Columns.Add("ShortName", typeof(string));
        dtGroups.Columns.Add("UserId", typeof(Guid));
        dtGroups.Columns.Add("Clicked", typeof(int));
        dtGroups.Columns.Add("CategoryId", typeof(Guid));
        dtGroups.Columns.Add("CreatedAt", typeof(DateTime));
        foreach (var linkGroup in entites)
        {
            var row = dtGroups.NewRow();
            row["LinkGroupId"] = linkGroup.LinkGroupId;
            row["Name"] = linkGroup.Name;
            row["Description"] = linkGroup.Description;
            row["ShortName"] = linkGroup.ShortName;
            row["UserId"] = linkGroup.User.UserId;
            row["Clicked"] = linkGroup.Clicked;
            row["CategoryId"] = linkGroup.Category.CategoryId;
            row["CreatedAt"] = linkGroup.CreatedAt;
            dtGroups.Rows.Add(row);
        }

        await using var connection = new SqlConnection(connectionString);
        var isSuccess = await connection.WriteBulkToDatabaseAsync(dtGroups);
        return isSuccess;
    }

    public override async Task<LinkGroup> InsertAsync(LinkGroup entity)
    {
        await using var connection = new SqlConnection(connectionString);
        entity.CreatedAt = DateTime.Now;
        entity.LinkGroupId = Guid.NewGuid();
        var sql =
            $"INSERT INTO LinkGroups(LinkGroupId,Name, Description,ShortName,UserId,Clicked,CategoryId,CreatedAt)" +
            $"VALUES (@lgId,@name,@desc,@shortName,@userId,@clicked,@catId,@created);";

        await connection.ExecuteAsync(sql, new
        {
            lgId = entity.LinkGroupId,
            name = entity.Name,
            desc = entity.Description,
            shortName = entity.ShortName,
            userId = entity.User.UserId,
            clicked = 0,
            catId = entity.Category.CategoryId,
            created = entity.CreatedAt
        });

        if (entity.Links != null && entity.Links.Any())
            await SaveLinksToDatabaseAsync(entity.LinkGroupId.ToString(), entity.Links.ToArray());

        return entity;
    }

    private async Task SaveLinksToDatabaseAsync(string linkGroupId, Link[] links)
    {
        await using var connection = new SqlConnection(connectionString);
        var sql =
            $"INSERT INTO Links(LinkId,Name, Url,LinkGroupId)" +
            $"VALUES (@lid,@name,@url,@lgId)";
        foreach (var link in links)
        {
            await connection.ExecuteAsync(sql, new
            {
                lid = Guid.NewGuid().ToString(),
                name = link.Name,
                url = link.Url,
                lgId = linkGroupId
            });
        }
    }

    public override async Task<PaginatedList<LinkGroup>> SearchAsync(int page, int pageSize, string query = "")
    {
        await using var connection = new SqlConnection(connectionString);
        var sql = "SELECT G.LinkGroupId, G.Name, G.Description, G.ShortName, " +
                  "G.UserId,G.Clicked,G.CategoryId,G.CreatedAt, C.CategoryId, C.Name, L.LinkId, L.Name, L.Url " +
                  "FROM LinkGroups G " +
                  "JOIN Categories C ON G.CategoryId = C.CategoryId " +
                  "LEFT JOIN Links L on L.LinkGroupId=G.LinkGroupId ";

        if (!string.IsNullOrEmpty(query))
            sql +=
                $"WHERE G.Name LIKE '%{query}%' OR G.Description LIKE '%{query}%' OR G.ShortName LIKE '%{query}%' OR C.Name LIKE '%{query}%'";

        var grid = await connection.QueryMultipleAsync(sql);
        var lookup = new Dictionary<Guid, LinkGroup>();
        grid.Read<LinkGroup, Category, Link, LinkGroup>((linkGroup, category, link) =>
        {
            linkGroup.Category = category;
            if (!lookup.TryGetValue(linkGroup.LinkGroupId, out _))
                lookup.Add(linkGroup.LinkGroupId, linkGroup);
            if (link == null) return linkGroup;
            lookup[linkGroup.LinkGroupId].Links.Add(link);
            return linkGroup;
        }, splitOn: "CategoryId,LinkId");

        return PaginatedList<LinkGroup>.Create(lookup.Values.AsQueryable(), page, pageSize, query);
    }

    public override async Task<LinkGroup> DetailsAsync(string entityId)
    {
        await using var connection = new SqlConnection(connectionString);
        var query =
            "SELECT G.LinkGroupId, G.Name, G.Description, G.ShortName, G.UserId,G.Clicked,G.CategoryId,G.CreatedAt " +
            "FROM LinkGroups G WHERE G.LinkGroupId=@entityId;" +
            "SELECT P.CategoryId, P.Name FROM Categories P " +
            "JOIN LinkGroups S on S.CategoryId=P.CategoryId WHERE S.LinkGroupId=@entityId;" +
            "SELECT L.LinkId, L.Name, L.Url, L.LinkGroupId " +
            "FROM Links L JOIN LinkGroups S on S.LinkGroupId=L.LinkGroupId WHERE S.LinkGroupId=@entityId;" +
            "SELECT U.UserId, U.Email, U.Fullname FROM Users U " +
            "JOIN LinkGroups P ON P.UserId=U.UserId WHERE P.LinkGroupId=@entityId;";

        var result = await connection.QueryMultipleAsync(query, new { entityId });
        var linkGroup = await result.ReadSingleAsync<LinkGroup>();
        var category = await result.ReadSingleAsync<Category>();
        var links = await result.ReadAsync<Link>();
        var user = await result.ReadSingleAsync<SulUser>();

        linkGroup.Category = category;
        linkGroup.Links = links.ToList();
        linkGroup.User = user;

        return linkGroup;
    }

    public async Task<List<LinkGroup>> GetFromSpecificUserAsync(string userId)
    {
        await using var connection = new SqlConnection(connectionString);
        var sql = "SELECT G.LinkGroupId, G.Name, G.Description, G.ShortName, " +
                  "G.UserId,G.Clicked,G.CategoryId,G.CreatedAt, C.CategoryId, C.Name, " +
                  "L.LinkId, L.Name, L.Url FROM LinkGroups G " +
                  "JOIN Categories C ON G.CategoryId = C.CategoryId " +
                  "LEFT JOIN Links L on L.LinkGroupId=G.LinkGroupId " +
                  "WHERE G.UserId=@userId";

        var grid = await connection.QueryMultipleAsync(sql, new { userId });
        var lookup = new Dictionary<Guid, LinkGroup>();
        grid.Read<LinkGroup, Category, Link, LinkGroup>((linkGroup, category, link) =>
        {
            linkGroup.Category = category;
            if (!lookup.TryGetValue(linkGroup.LinkGroupId, out _))
                lookup.Add(linkGroup.LinkGroupId, linkGroup);
            if (link == null) return linkGroup;
            lookup[linkGroup.LinkGroupId].Links.Add(link);
            return linkGroup;
        }, splitOn: "CategoryId,LinkId");
        return lookup.Values.ToList();
    }

    public override async Task<bool> UpdateAsync(LinkGroup entity)
    {
        await using var connection = new SqlConnection(connectionString);
        var query = "UPDATE LinkGroups SET Name=@name," +
                    "Description=@desc, ShortName=@shortName, UserId=@userId," +
                    "Clicked=@clicked, CategoryId=@catId " +
                    "WHERE LinkGroupId=@linkGroupId";
        var resultAffected = await connection.ExecuteAsync(query, new
        {
            name = entity.Name,
            desc = entity.Description,
            shortName = entity.ShortName,
            linkGroupId = entity.LinkGroupId,
            userId = entity.User.UserId,
            clicked = entity.Clicked,
            catId = entity.Category.CategoryId
        });

        if (!entity.Links.Any()) return resultAffected > 0;

        query = "DELETE FROM Links WHERE LinkGroupId=@linkGroupId";
        await connection.ExecuteAsync(query, new
        {
            linkGroupId = entity.LinkGroupId
        });
        await SaveLinksToDatabaseAsync(entity.LinkGroupId.ToString(), entity.Links.ToArray());
        return resultAffected > 0;
    }

    public async Task<LinkGroup> GetLinkFromShortNameAsync(string shortName)
    {
        await using var connection = new SqlConnection(connectionString);

        var query = "SELECT G.LinkGroupId FROM LinkGroups G WHERE G.ShortName=@shortName";
        var linkGroupId = await connection.QuerySingleAsync<Guid>(query, new { shortName });

        var group = await DetailsAsync(linkGroupId.ToString());
        group.Clicked += 1;

        try
        {
            await UpdateAsync(group);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return null!;
        }

        return group;
    }

    public async Task<bool> AddLinksToLinkGroupAsync(string linkGroupId, List<Link> links)
    {
        try
        {
            //delete all the links in the group
            await using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Links WHERE LinkGroupId=@currentId";
            var resultAffected = await connection.ExecuteAsync(sql, new
            {
                currentId = linkGroupId
            });
            await SaveLinksToDatabaseAsync(linkGroupId, links.ToArray());
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public override async Task<List<LinkGroup>> GetAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        var sql =
            "SELECT G.LinkGroupId, G.Name, G.Description, G.ShortName, " +
            "G.UserId,G.Clicked,G.CategoryId,G.CreatedAt, C.CategoryId, C.Name FROM LinkGroups G " +
            "JOIN Categories C ON G.CategoryId = C.CategoryId ";

        var grid = await connection.QueryMultipleAsync(sql);
        var lookup = new Dictionary<Guid, LinkGroup>();
        grid.Read<LinkGroup, Category, LinkGroup>((linkGroup, category) =>
        {
            linkGroup.Category = category;
            if (!lookup.TryGetValue(linkGroup.LinkGroupId, out _))
                lookup.Add(linkGroup.LinkGroupId, linkGroup);
            return linkGroup;
        }, splitOn: "CategoryId");
        return lookup.Values.ToList();
    }

    public async Task<List<LinkGroup>> SearchAsync(string query = "")
    {
        await using var connection = new SqlConnection(connectionString);
        var sql = "SELECT G.LinkGroupId, G.Name, G.Description, G.ShortName, " +
                  "G.UserId,G.Clicked,G.CategoryId,G.CreatedAt, C.CategoryId, C.Name, L.LinkId, L.Name, L.Url " +
                  "FROM LinkGroups G " +
                  "JOIN Categories C ON G.CategoryId = C.CategoryId " +
                  "LEFT JOIN Links L on L.LinkGroupId=G.LinkGroupId ";

        if (!string.IsNullOrEmpty(query))
            sql +=
                $"WHERE G.Name LIKE '%{query}%' OR G.Description LIKE '%{query}%' OR G.ShortName LIKE '%{query}%' OR C.Name LIKE '%{query}%'";

        var grid = await connection.QueryMultipleAsync(sql);
        var lookup = new Dictionary<Guid, LinkGroup>();
        grid.Read<LinkGroup, Category, Link, LinkGroup>((linkGroup, category, link) =>
        {
            linkGroup.Category = category;
            if (!lookup.TryGetValue(linkGroup.LinkGroupId, out _))
                lookup.Add(linkGroup.LinkGroupId, linkGroup);
            if (link == null) return linkGroup;
            lookup[linkGroup.LinkGroupId].Links.Add(link);
            return linkGroup;
        }, splitOn: "CategoryId,LinkId");
        return [..lookup.Values];
    }

    public override async Task<bool> DeleteAsync(string entityId)
    {
        await using var connection = new SqlConnection(connectionString);
        var sql = "DELETE FROM Links WHERE LinkGroupId=@linkGroupId";
        var resultAffected = await connection.ExecuteAsync(sql, new
        {
            linkGroupId = entityId
        });

        sql = "DELETE FROM LinkGroups WHERE LinkGroupId=@linkGroupId";
        resultAffected = await connection.ExecuteAsync(sql, new
        {
            linkGroupId = entityId
        });

        return resultAffected > 0;
    }
}