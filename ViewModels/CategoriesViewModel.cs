using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Models;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _canModify;

    public CategoriesViewModel(ICategoryService categoryService, IAuthorizationService authorizationService)
    {
        _categoryService = categoryService;
        _authorizationService = authorizationService;

        CanModify = _authorizationService.CanCreate("Categories");
        _ = LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = string.IsNullOrWhiteSpace(SearchText)
                ? await _categoryService.GetAllAsync()
                : await _categoryService.SearchAsync(SearchText);

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Add()
    {
        SelectedCategory = new Category { Name = string.Empty, IsActive = true };
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedCategory == null) return;

        try
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory.Name))
            {
                MessageBox.Show("Category name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCategory.CategoryId == 0)
            {
                await _categoryService.CreateAsync(SelectedCategory);
                MessageBox.Show("Category created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _categoryService.UpdateAsync(SelectedCategory);
                MessageBox.Show("Category updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SelectedCategory = null;
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedCategory == null || SelectedCategory.CategoryId == 0) return;

        var result = MessageBox.Show("Are you sure you want to delete this category?", "Confirm Delete", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _categoryService.DeleteAsync(SelectedCategory.CategoryId);
            MessageBox.Show("Category deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SelectedCategory = null;
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedCategory = null;
    }
}
