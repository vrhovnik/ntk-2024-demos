using NTK24.Models;

namespace NTK24.Interfaces;

public interface IUserService
{
    Task<SulUser?> LoginAsync(string username, string password);
    Task<SulUser> FindAsync(string email);
    Task<SulUser> InsertAsync(SulUser newUser);
    Task<bool> BulkInsertAsync(IEnumerable<SulUser> users);
}