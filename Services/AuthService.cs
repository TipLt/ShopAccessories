using BL5_PRN212_MustPass_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace BL5_PRN212_MustPass_Project.Services;

public class AuthService : IAuthService
{
    private User? _currentUser;

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public async Task<User?> LoginAsync(string username, string password)
    {
        using var context = new ShopAccessoriesContext();
        var user = await context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Username == username && u.Password == password && u.IsActive);

        if (user != null)
        {
            _currentUser = user;
        }

        return user;
    }

    public void Logout()
    {
        _currentUser = null;
    }
}
