using BL5_PRN212_MustPass_Project.Models;

namespace BL5_PRN212_MustPass_Project.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
