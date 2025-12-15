using BL5_PRN212_MustPass_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace BL5_PRN212_MustPass_Project.Services;

public class CustomerService : ICustomerService
{
    private readonly IAuthorizationService _authorizationService;

    public CustomerService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        _authorizationService.EnsureCanRead("Customers");
        using var context = new ShopAccessoriesContext();
        return await context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        _authorizationService.EnsureCanRead("Customers");
        using var context = new ShopAccessoriesContext();
        return await context.Customers.FindAsync(id);
    }

    public async Task<List<Customer>> SearchAsync(string searchTerm)
    {
        _authorizationService.EnsureCanRead("Customers");
        using var context = new ShopAccessoriesContext();
        return await context.Customers
            .Where(c => c.IsActive && (c.Name.Contains(searchTerm) || 
                   (c.Email != null && c.Email.Contains(searchTerm)) ||
                   (c.Phone != null && c.Phone.Contains(searchTerm))))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _authorizationService.EnsureCanCreate("Customers");
        using var context = new ShopAccessoriesContext();
        customer.IsActive = true;
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _authorizationService.EnsureCanUpdate("Customers");
        using var context = new ShopAccessoriesContext();
        context.Customers.Update(customer);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _authorizationService.EnsureCanDelete("Customers");
        using var context = new ShopAccessoriesContext();
        var customer = await context.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.IsActive = false;
            await context.SaveChangesAsync();
        }
    }
}
