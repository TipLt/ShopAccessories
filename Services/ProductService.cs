using BL5_PRN212_MustPass_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace BL5_PRN212_MustPass_Project.Services;

public class ProductService : IProductService
{
    private readonly IAuthorizationService _authorizationService;

    public ProductService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        _authorizationService.EnsureCanRead("Products");
        using var context = new ShopAccessoriesContext();
        return await context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        _authorizationService.EnsureCanRead("Products");
        using var context = new ShopAccessoriesContext();
        return await context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<List<Product>> SearchAsync(string searchTerm)
    {
        _authorizationService.EnsureCanRead("Products");
        using var context = new ShopAccessoriesContext();
        return await context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && (p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _authorizationService.EnsureCanCreate("Products");
        using var context = new ShopAccessoriesContext();
        
        var existingCode = await context.Products.AnyAsync(p => p.Code == product.Code);
        if (existingCode)
            throw new InvalidOperationException($"Product code '{product.Code}' already exists");

        product.IsActive = true;
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _authorizationService.EnsureCanUpdate("Products");
        using var context = new ShopAccessoriesContext();
        
        var existingCode = await context.Products
            .AnyAsync(p => p.Code == product.Code && p.ProductId != product.ProductId);
        if (existingCode)
            throw new InvalidOperationException($"Product code '{product.Code}' already exists");

        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _authorizationService.EnsureCanDelete("Products");
        using var context = new ShopAccessoriesContext();
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            product.IsActive = false;
            await context.SaveChangesAsync();
        }
    }
}
