using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Models;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _canModify;

    [ObservableProperty]
    private bool _canCreate;

    public CustomersViewModel(ICustomerService customerService, IAuthorizationService authorizationService)
    {
        _customerService = customerService;
        _authorizationService = authorizationService;

        CanModify = _authorizationService.CanUpdate("Customers");
        CanCreate = _authorizationService.CanCreate("Customers");
        _ = LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = string.IsNullOrWhiteSpace(SearchText)
                ? await _customerService.GetAllAsync()
                : await _customerService.SearchAsync(SearchText);

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Add()
    {
        SelectedCustomer = new Customer { Name = string.Empty, IsActive = true };
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedCustomer == null) return;

        try
        {
            if (string.IsNullOrWhiteSpace(SelectedCustomer.Name))
            {
                MessageBox.Show("Customer name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCustomer.CustomerId == 0)
            {
                await _customerService.CreateAsync(SelectedCustomer);
                MessageBox.Show("Customer created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _customerService.UpdateAsync(SelectedCustomer);
                MessageBox.Show("Customer updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SelectedCustomer = null;
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedCustomer == null || SelectedCustomer.CustomerId == 0) return;

        var result = MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _customerService.DeleteAsync(SelectedCustomer.CustomerId);
            MessageBox.Show("Customer deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SelectedCustomer = null;
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedCustomer = null;
    }
}
