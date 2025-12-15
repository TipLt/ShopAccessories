using BL5_PRN212_MustPass_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace BL5_PRN212_MustPass_Project.Services;

public class UserService : IUserService
{
    private readonly IAuthorizationService _authorizationService;

    public UserService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<List<User>> GetAllAsync()
    {
        _authorizationService.EnsureCanRead("Users");
        using var context = new ShopAccessoriesContext();
        return await context.Users
            .AsNoTracking() // ? Ensure fresh data from database
            .Include(u => u.Customer)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        _authorizationService.EnsureCanRead("Users");
        using var context = new ShopAccessoriesContext();
        return await context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User> CreateAsync(User user)
    {
        _authorizationService.EnsureCanCreate("Users");
        using var context = new ShopAccessoriesContext();

        var existingUsername = await context.Users.AnyAsync(u => u.Username == user.Username);
        if (existingUsername)
            throw new InvalidOperationException($"Username '{user.Username}' already exists");

        if (user.Role == "Customer" && user.CustomerId == null)
            throw new InvalidOperationException("Customer role must be linked to a customer");

        if ((user.Role == "Admin" || user.Role == "Staff") && user.CustomerId != null)
            throw new InvalidOperationException("Admin and Staff roles cannot be linked to a customer");

        user.IsActive = true;
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _authorizationService.EnsureCanUpdate("Users");
        using var context = new ShopAccessoriesContext();

        var existingUsername = await context.Users
            .AnyAsync(u => u.Username == user.Username && u.UserId != user.UserId);
        if (existingUsername)
            throw new InvalidOperationException($"Username '{user.Username}' already exists");

        if (user.Role == "Customer" && user.CustomerId == null)
            throw new InvalidOperationException("Customer role must be linked to a customer");

        if ((user.Role == "Admin" || user.Role == "Staff") && user.CustomerId != null)
            throw new InvalidOperationException("Admin and Staff roles cannot be linked to a customer");

        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _authorizationService.EnsureCanDelete("Users");
        using var context = new ShopAccessoriesContext();
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await context.SaveChangesAsync();
        }
    }
}
