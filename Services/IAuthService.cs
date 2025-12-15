using BL5_PRN212_MustPass_Project.Models;

namespace BL5_PRN212_MustPass_Project.Services;

public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
    void Logout();
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
}
