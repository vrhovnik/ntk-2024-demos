using NTK24.Models;

namespace NTK24.Interfaces;

public interface ISimpleStatsService
{
    Task<StatInfo> GetForLinkGroupAsync(string linkGroupId);
    Task<bool> SaveAsync(StatInfo statInfo);
}