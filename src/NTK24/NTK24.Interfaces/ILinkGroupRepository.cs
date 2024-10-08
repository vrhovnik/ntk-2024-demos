﻿using NTK24.Models;

namespace NTK24.Interfaces;

public interface ILinkGroupRepository : IDataRepository<LinkGroup>
{
    Task<List<LinkGroup>> GetFromSpecificUserAsync(string userId);
    Task<LinkGroup> GetLinkFromShortNameAsync(string shortName);
    Task<bool> AddLinksToLinkGroupAsync(string linkGroupId, List<Link> links);
}