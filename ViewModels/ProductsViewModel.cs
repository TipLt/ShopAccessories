using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Models;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _canModify;

    public ProductsViewModel(
        IProductService productService, 
        ICategoryService categoryService,
        IAuthorizationService authorizationService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _authorizationService = authorizationService;

        CanModify = _authorizationService.CanCreate("Products");
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadProductsAsync();
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            var products = string.IsNullOrWhiteSpace(SearchText)
                ? await _productService.GetAllAsync()
                : await _productService.SearchAsync(SearchText);

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
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
        SelectedProduct = new Product 
        { 
            Code = string.Empty, 
            Name = string.Empty, 
            IsActive = true,
            Quantity = 0,
            Price = 0
        };
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedProduct == null) return;

        try
        {
            if (string.IsNullOrWhiteSpace(SelectedProduct.Code))
            {
                MessageBox.Show("Product code is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedProduct.Name))
            {
                MessageBox.Show("Product name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProduct.CategoryId == 0)
            {
                MessageBox.Show("Please select a category", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProduct.Price <= 0)
            {
                MessageBox.Show("Price must be greater than 0", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProduct.ProductId == 0)
            {
                await _productService.CreateAsync(SelectedProduct);
                MessageBox.Show("Product created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _productService.UpdateAsync(SelectedProduct);
                MessageBox.Show("Product updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SelectedProduct = null;
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedProduct == null || SelectedProduct.ProductId == 0) return;

        var result = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _productService.DeleteAsync(SelectedProduct.ProductId);
            MessageBox.Show("Product deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SelectedProduct = null;
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedProduct = null;
    }
}
