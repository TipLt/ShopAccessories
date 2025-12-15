using BL5_PRN212_MustPass_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace BL5_PRN212_MustPass_Project.Services;

public class CategoryService : ICategoryService
{
    private readonly IAuthorizationService _authorizationService;

    public CategoryService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        _authorizationService.EnsureCanRead("Categories");
        using var context = new ShopAccessoriesContext();
        return await context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        _authorizationService.EnsureCanRead("Categories");
        using var context = new ShopAccessoriesContext();
        return await context.Categories.FindAsync(id);
    }

    public async Task<List<Category>> SearchAsync(string searchTerm)
    {
        _authorizationService.EnsureCanRead("Categories");
        using var context = new ShopAccessoriesContext();
        return await context.Categories
            .Where(c => c.IsActive && c.Name.Contains(searchTerm))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _authorizationService.EnsureCanCreate("Categories");
        using var context = new ShopAccessoriesContext();
        category.IsActive = true;
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _authorizationService.EnsureCanUpdate("Categories");
        using var context = new ShopAccessoriesContext();
        context.Categories.Update(category);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _authorizationService.EnsureCanDelete("Categories");
        using var context = new ShopAccessoriesContext();
        var category = await context.Categories.FindAsync(id);
        if (category != null)
        {
            category.IsActive = false;
            await context.SaveChangesAsync();
        }
    }
}
