using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Models;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class MyProfileViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private Customer? _customer;

    public MyProfileViewModel(ICustomerService customerService, IAuthService authService)
    {
        _customerService = customerService;
        _authService = authService;
        _ = LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var customerId = _authService.CurrentUser?.CustomerId;
            if (customerId == null)
            {
                MessageBox.Show("Customer information not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Customer = await _customerService.GetByIdAsync(customerId.Value);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task LoadProfile()
    {
        await LoadProfileAsync();
    }
}
