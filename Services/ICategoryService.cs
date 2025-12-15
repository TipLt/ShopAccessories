using BL5_PRN212_MustPass_Project.Models;

namespace BL5_PRN212_MustPass_Project.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<List<Category>> SearchAsync(string searchTerm);
    Task<Category> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}
