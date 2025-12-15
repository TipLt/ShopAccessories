using BL5_PRN212_MustPass_Project.Models;

namespace BL5_PRN212_MustPass_Project.Services;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<List<Customer>> SearchAsync(string searchTerm);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
}
