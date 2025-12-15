using BL5_PRN212_MustPass_Project.Models;

namespace BL5_PRN212_MustPass_Project.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> SearchAsync(string searchTerm);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
